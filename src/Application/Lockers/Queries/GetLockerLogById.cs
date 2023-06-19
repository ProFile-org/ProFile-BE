using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Logging;
using AutoMapper;
using Domain.Entities.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Lockers.Queries;

public class GetLockerLogById
{
    public record Query : IRequest<LockerLogDto>
    {
        public Guid LogId { get; init; }
        public string CurrentUserRole { get; init; } = null!;
        public Guid? CurrentStaffRoomId { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, LockerLogDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<LockerLogDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var log = await _context.LockerLogs
                .Include(x => x.Object)
                .ThenInclude(x => x!.Room)
                .Include(x => x.User)
                .ThenInclude(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.LogId), cancellationToken);

            if (log is null)
            {
                throw new KeyNotFoundException("Log does not exist.");
            }

            if (request.CurrentUserRole.IsStaff()
                && (request.CurrentStaffRoomId is null || !LockerInSameRoom(log, request.CurrentStaffRoomId.Value)))
            {
                throw new UnauthorizedAccessException("User cannot access this resource.");
            }

            return _mapper.Map<LockerLogDto>(log);
        }

        private static bool LockerInSameRoom(
            LockerLog log,
            Guid roomId)
            => log.BaseRoom!.Id == roomId;
    }
}