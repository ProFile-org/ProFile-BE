using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Lockers.Commands.Remove;

public record Command : IRequest<LockerDto>
{
    public Guid LockerId { get; init; }
}