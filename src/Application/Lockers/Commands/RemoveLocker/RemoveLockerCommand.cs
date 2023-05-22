using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Physical;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Lockers.Commands.RemoveLocker;

public record RemoveLockerCommand : IRequest<LockerDto>
{
    public Guid LockerId { get; init; }
}

public class RemoveLockerCommandHandler : IRequestHandler<RemoveLockerCommand, LockerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public RemoveLockerCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<LockerDto> Handle(RemoveLockerCommand request, CancellationToken cancellationToken)
    {
        var locker = await _context.Lockers
            .Include(x => x.Room)
            .FirstOrDefaultAsync(x => x.Id.Equals(request.LockerId) && x.IsAvailable == true, cancellationToken);
        if (locker is null)
        {
            throw new KeyNotFoundException("Locker does not exist.");
        }

        locker.IsAvailable = false;
        locker.Room.NumberOfLockers -= 1;
        var result = _context.Lockers.Update(locker);
        _context.Rooms.Update(locker.Room);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<LockerDto>(result.Entity);
    }
}