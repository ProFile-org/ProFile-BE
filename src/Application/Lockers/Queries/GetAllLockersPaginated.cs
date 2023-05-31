using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;

namespace Application.Lockers.Queries;

public class GetAllLockersPaginated
{
    public record Query : IRequest<PaginatedList<LockerDto>>
    {
        public Guid? RoomId { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, PaginatedList<LockerDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<LockerDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var lockers = _context.Lockers.AsQueryable();

            if (request.RoomId is not null)
            {
                lockers = lockers.Where(x => x.Room.Id == request.RoomId);
            }
            
            var sortBy = request.SortBy;
            if (sortBy is null || !sortBy.MatchesPropertyName<LockerDto>())
            {
                sortBy = nameof(LockerDto.Id);
            }
            var sortOrder = request.SortOrder ?? "asc";
            var pageNumber = request.Page ?? 1;
            var sizeNumber = request.Size ?? 5;

            var result = await lockers
                .ProjectTo<LockerDto>(_mapper.ConfigurationProvider)
                .OrderByCustom(sortBy, sortOrder)
                .PaginatedListAsync(pageNumber, sizeNumber);

            return result;
        }
    }
}