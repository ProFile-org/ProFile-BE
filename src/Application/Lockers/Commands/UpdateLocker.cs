using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Lockers.Commands;

public class UpdateLocker
{
    public record Command : IRequest<LockerDto>
    {
        public Guid LockerId { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public int Capacity { get; init; }
    }
}