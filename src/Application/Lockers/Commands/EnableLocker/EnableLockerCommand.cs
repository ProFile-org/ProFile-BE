using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Lockers.Commands.EnableLocker;

public record EnableLockerCommand : IRequest<LockerDto>
{
    public Guid LockerId { get; init; }
}

public class EnableLockerCommandHandler : IRequestHandler<EnableLockerCommand, LockerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public EnableLockerCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<LockerDto> Handle(EnableLockerCommand request, CancellationToken cancellationToken)
    {
        var locker = await _context.Lockers
            .FirstOrDefaultAsync(x => x.Id.Equals(request.LockerId), cancellationToken);
        if (locker is null)
        {
            throw new KeyNotFoundException("Locker does not exist.");
        }

        if (locker.IsAvailable)
        {
            throw new ConflictException("Locker has already been enabled.");
        }
        
        locker.IsAvailable = true;
        var result = _context.Lockers.Update(locker);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<LockerDto>(result.Entity);
    }
}
