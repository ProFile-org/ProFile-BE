using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Logging;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Borrows.Queries;

public class GetAllRequestLogsPaginated
{
    public record Query : IRequest<PaginatedList<RequestLogDto>>
    {
        public string? SearchTerm { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, PaginatedList<RequestLogDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<RequestLogDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var logs = _context.RequestLogs
                .Include(x => x.Object)
                .AsQueryable();

            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                logs = logs.Where(x => 
                    x.Action.Trim().ToLower().Contains(request.SearchTerm.Trim().ToLower()));
            }

            var sortBy = request.SortBy;
            if (sortBy is null || !sortBy.MatchesPropertyName<RequestLogDto>())
            {
                sortBy = nameof(RequestLogDto.Time);
            }

            var sortOrder = request.SortOrder ?? "desc";
            var pageNumber = request.Page is null or <= 0 ? 1 : request.Page;
            var sizeNumber = request.Size is null or <= 0 ? 5 : request.Size;

            var count = await logs.CountAsync(cancellationToken);
            var list  = await logs
                .Paginate(pageNumber.Value, sizeNumber.Value)
                .OrderByCustom(sortBy, sortOrder)
                .ToListAsync(cancellationToken);

            var result = _mapper.Map<List<RequestLogDto>>(list);

            return new PaginatedList<RequestLogDto>(result, count, pageNumber.Value, sizeNumber.Value);
        }
    }
}
