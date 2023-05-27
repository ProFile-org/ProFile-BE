using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Lockers.Commands.RemoveLocker;

public record RemoveLockerCommand : IRequest<LockerDto>
{
    public Guid LockerId { get; init; }
}