using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;

namespace Application.Rooms.Queries;

public class GetAllRoomsPaginated
{
    public record Query : IRequest<PaginatedList<RoomDto>>
    {
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, PaginatedList<RoomDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<RoomDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var rooms = _context.Rooms.AsQueryable();

            var sortBy = request.SortBy;
            if (sortBy is null || !sortBy.MatchesPropertyName<RoomDto>())
            {
                sortBy = nameof(RoomDto.Id);
            }

            var sortOrder = request.SortOrder ?? "asc";
            var pageNumber = request.Page ?? 1;
            var sizeNumber = request.Size ?? 5;
            
            var result = await rooms
                .ProjectTo<RoomDto>(_mapper.ConfigurationProvider)
                .OrderByCustom(sortBy, sortOrder)
                .PaginatedListAsync(pageNumber, sizeNumber);

            return result;
        }
    }
}