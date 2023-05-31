using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Lockers.Commands;

public class RemoveLocker
{
    public record Command : IRequest<LockerDto>
    {
        public Guid LockerId { get; init; }
    }
}