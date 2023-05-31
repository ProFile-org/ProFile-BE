using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Lockers.Queries;

public class GetAllLockersPaginated
{
    public record Query : IRequest<PaginatedList<LockerDto>>
    {
        public Guid? RoomId { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
    }
}