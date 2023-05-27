using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using MediatR;

namespace Application.Staffs.Queries.GetAllPaginated;

public class Query : IRequest<PaginatedList<StaffDto>>
{
    public string? SearchTerm { get; set; }
    public int? Page { get; set; }
    public int? Size { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}