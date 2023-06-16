using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Logging;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Queries;

public class GetRoomLogById
{
    public record Query : IRequest<RoomLogDto>
    {
        public Guid LogId { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, RoomLogDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<RoomLogDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var log = await _context.RoomLogs
                .Include(x => x.Object)
                .Include(x => x.User)
                .ThenInclude(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == request.LogId, cancellationToken);

            if (log is null)
            {
                throw new KeyNotFoundException("Log does not exist.");
            }

            return _mapper.Map<RoomLogDto>(log);
        }
    }
}