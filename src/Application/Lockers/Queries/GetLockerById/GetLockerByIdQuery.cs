using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Lockers.Queries.GetLockerById;

public record GetLockerByIdQuery : IRequest<LockerDto>
{
    public Guid LockerId { get; init; }
}