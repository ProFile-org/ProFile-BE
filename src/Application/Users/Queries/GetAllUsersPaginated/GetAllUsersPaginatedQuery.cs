using Application.Common.Models;
using MediatR;

namespace Application.Users.Queries.GetAllUsersPaginated;

public record GetAllUsersPaginatedQuery : IRequest<PaginatedList<UserDto>>
{
    public Guid? DepartmentId { get; init; }
    public string? SearchTerm { get; init; }
    public int? Page { get; init; }
    public int? Size { get; init; }
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; }
}