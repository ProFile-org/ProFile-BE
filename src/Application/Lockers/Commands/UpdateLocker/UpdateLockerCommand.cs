using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Lockers.Commands.UpdateLocker;

public record UpdateLockerCommand : IRequest<LockerDto>
{
    public Guid LockerId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int Capacity { get; init; }
}