using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Rooms.Queries.GetAllRoomPaginated;

public record GetAllRoomsPaginatedQuery : IRequest<PaginatedList<RoomDto>>
{
    public int? Page { get; init; }
    public int? Size { get; init; }
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; }
}