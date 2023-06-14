using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Logging;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Queries;

public class GetAllRoomLogsPaginated
{
    public record Query : IRequest<PaginatedList<RoomLogDto>>
    {
        public string? SearchTerm { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, PaginatedList<RoomLogDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<RoomLogDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var logs = _context.RoomLogs
                .Include(x => x.Object)
                .Include(x => x.User)
                .ThenInclude(x => x.Department)
                .AsQueryable();
            
            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                logs = logs.Where(x =>
                    x.Action.ToLower().Contains(request.SearchTerm.ToLower()));
            }
            
            var sortBy = request.SortBy;
            if (sortBy is null || !sortBy.MatchesPropertyName<RoomLogDto>())
            {
                sortBy = nameof(RoomLogDto.Time);
            }
            var sortOrder = request.SortOrder ?? "desc";
            var pageNumber = request.Page is null or <= 0 ? 1 : request.Page;
            var sizeNumber = request.Size is null or <= 0 ? 5 : request.Size;

            var count = await logs.CountAsync(cancellationToken);
            var list  = await logs
                .OrderByCustom(sortBy, sortOrder)
                .Paginate(pageNumber.Value, sizeNumber.Value)
                .ToListAsync(cancellationToken);
            
            var result = _mapper.Map<List<RoomLogDto>>(list);

            return new PaginatedList<RoomLogDto>(result, count, pageNumber.Value, sizeNumber.Value);
        }
    }
}