using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Logging;
using AutoMapper;
using Domain.Entities.Logging;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Queries;

public class GetAllRoomLogsPaginated
{
    public record Query : IRequest<PaginatedList<RoomLogDto>>
    {
        public Guid? RoomId { get; init; }
        public string? SearchTerm { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
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
            
            if (request.RoomId is not null)
            {
                logs = logs.Where(x => x.Object!.Id == request.RoomId);
            }
            
            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                logs = logs.Where(x =>
                    x.Action.ToLower().Contains(request.SearchTerm.ToLower()));
            }
            
            return await logs
                .LoggingListPaginateAsync<Room, RoomLog, RoomLogDto>(
                    request.Page,
                    request.Size,
                    _mapper.ConfigurationProvider,
                    cancellationToken);
        }
    }
}