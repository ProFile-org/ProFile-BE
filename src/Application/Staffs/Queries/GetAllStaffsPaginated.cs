using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Staffs.Queries;

public class GetAllStaffsPaginated
{
    public class Query : IRequest<PaginatedList<StaffDto>>
    {
        public string? SearchTerm { get; set; }
        public int? Page { get; set; }
        public int? Size { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
    }
    
    public class QueryHandler : IRequestHandler<Query, PaginatedList<StaffDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<StaffDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var staffs = _context.Staffs
                .Include(x => x.Room)
                .Include(x => x.User)
                .ThenInclude(x => x.Department)
                .AsQueryable();
            
            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
               staffs = staffs.Where(x => x.User.Username.ToLower().Contains(request.SearchTerm.ToLower()));
            }

            var sortBy = request.SortBy;
            if (sortBy is null || !sortBy.MatchesPropertyName<StaffDto>())
            {
                sortBy = nameof(StaffDto.Id);
            }
            
            var sortOrder = request.SortOrder ?? "asc";
            var pageNumber = request.Page is null or <= 0 ? 1 : request.Page;
            var sizeNumber = request.Size is null or <= 0 ? 5 : request.Size;
            
            var list = await staffs
                .OrderByCustom(sortBy, sortOrder)
                .Paginate(pageNumber.Value,sizeNumber.Value)
                .ToListAsync(cancellationToken);

            var result = _mapper.Map<List<StaffDto>>(list);
            
            return new PaginatedList<StaffDto>(result, result.Count, pageNumber.Value, sizeNumber.Value);
        }
    }
}