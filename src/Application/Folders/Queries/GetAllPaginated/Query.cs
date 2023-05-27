using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Folders.Queries.GetAllPaginated;

public record Query : IRequest<PaginatedList<FolderDto>>
{
    public Guid? RoomId { get; init; }
    public Guid? LockerId { get; init; }
    public int? Page { get; init; }
    public int? Size { get; init; }
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; }
}