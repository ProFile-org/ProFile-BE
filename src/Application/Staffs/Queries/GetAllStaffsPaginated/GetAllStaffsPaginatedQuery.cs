using Application.Common.Models;
using Application.Users.Queries.Physical;
using MediatR;

namespace Application.Staffs.Queries.GetAllStaffsPaginated;

public class GetAllStaffsPaginatedQuery : IRequest<PaginatedList<StaffDto>>
{
    public string? SearchTerm { get; set; }
    public int? Page { get; set; }
    public int? Size { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}