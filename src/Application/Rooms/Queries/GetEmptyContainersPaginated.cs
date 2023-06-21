using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Queries;

public class GetEmptyContainersPaginated
{
    public record Query : IRequest<PaginatedList<EmptyLockerDto>>
    {
        public Guid RoomId { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, PaginatedList<EmptyLockerDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<EmptyLockerDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(x => x.Id == request.RoomId, cancellationToken);
            if (room is null)
            {
                throw new KeyNotFoundException("Room does not exist.");
            }

            var pageNumber = request.Page ?? 1;
            var sizeNumber = request.Size ?? 5;
            var lockers = _context.Lockers
                .Where(x => x.Room.Id == request.RoomId
                            && x.IsAvailable
                            && x.Folders.Any(y => y.Capacity > y.NumberOfDocuments && y.IsAvailable))
                .ProjectTo<EmptyLockerDto>(_mapper.ConfigurationProvider)
                .AsEnumerable()
                .ToList();
            
            lockers.ForEach(x => x.Folders = x.Folders.Where(y => y.Slot > 0));

            var result = new PaginatedList<EmptyLockerDto>(lockers.ToList(), lockers.Count, pageNumber, sizeNumber);
            return result;
        }
    }
}