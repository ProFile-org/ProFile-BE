using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Lockers.Commands.DisableLocker;

public record DisableLockerCommand : IRequest<LockerDto>
{
    public Guid LockerId { get; init; }
}

public class RemoveLockerCommandHandler : IRequestHandler<DisableLockerCommand, LockerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public RemoveLockerCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<LockerDto> Handle(DisableLockerCommand request, CancellationToken cancellationToken)
    {
        var locker = await _context.Lockers
            .FirstOrDefaultAsync(x => x.Id.Equals(request.LockerId), cancellationToken);
        if (locker is null)
        {
            throw new KeyNotFoundException("Locker does not exist.");
        }

        if (locker.IsAvailable == false)
        {
            throw new ConflictException("Locker has already been disabled.");
        }
        
        var canNotDisable = await _context.Documents
                                .CountAsync(x => x.Folder!.Locker.Id.Equals(request.LockerId), cancellationToken)
                            > 0;

        if (canNotDisable)
        {
            throw new InvalidOperationException("Locker cannot be disabled because it contains documents.");
        }
        
        var folders = _context.Folders.Where(x => x.Locker.Room.Id.Equals(locker.Id));
        
        foreach (var folder in folders)
        {
            folder.IsAvailable = false;
        }
        _context.Folders.UpdateRange(folders);
        
        locker.IsAvailable = false;
        var result = _context.Lockers.Update(locker);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<LockerDto>(result.Entity);
    }
}