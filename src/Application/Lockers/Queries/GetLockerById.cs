using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Lockers.Queries;

public class GetLockerById
{
    public record Query : IRequest<LockerDto>
    {
        public Guid LockerId { get; init; }
    }
}