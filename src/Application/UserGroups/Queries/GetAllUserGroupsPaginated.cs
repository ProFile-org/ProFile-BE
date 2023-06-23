using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Digital;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UserGroups.Queries;

public class GetAllUserGroupsPaginated
{
    public record Query : IRequest<PaginatedList<UserGroupDto>>
    {
        public string? SearchTerm { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
    };

    public class QueryHandler : IRequestHandler<Query, PaginatedList<UserGroupDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<UserGroupDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userGroups = _context.UserGroups.AsQueryable();
            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                userGroups = userGroups.Where(x =>
                    x.Name.ToLower().Trim().Contains(request.SearchTerm.ToLower().Trim()));
            }
            
            var sortBy = request.SortBy;
            if (sortBy is null || !sortBy.MatchesPropertyName<UserGroupDto>())
            {
                sortBy = nameof(UserGroupDto.Id);
            }
            var sortOrder = request.SortOrder ?? "asc";
            var pageNumber = request.Page is null or <= 0 ? 1 : request.Page;
            var sizeNumber = request.Size is null or <= 0 ? 5 : request.Size;

            var count = await userGroups.CountAsync(cancellationToken);
            var list  = await userGroups
                .OrderByCustom(sortBy, sortOrder)
                .Paginate(pageNumber.Value, sizeNumber.Value)
                .ToListAsync(cancellationToken);
            
            var result = _mapper.Map<List<UserGroupDto>>(list);

            return new PaginatedList<UserGroupDto>(result, count, pageNumber.Value, sizeNumber.Value);
        }
    } 
}