using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Logging;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Logging;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Lockers.Queries;

public class GetAllLockerLogsPaginated
{
    public record Query : IRequest<PaginatedList<LockerLogDto>>
    {
        public string CurrentUserRole { get; init; } = null!;
        public Guid CurrentUserDepartmentId { get; init; }
        public string? SearchTerm { get; init; }
        public Guid? LockerId { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, PaginatedList<LockerLogDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<LockerLogDto>> Handle(Query request, CancellationToken cancellationToken)
        {

            if (request.CurrentUserRole.IsStaff())
            {
                if (request.LockerId is null)
                {
                    throw new UnauthorizedAccessException("User cannot access this resource.");
                }

                var currentRoom = await GetRoomByDepartmentIdAsync(request.CurrentUserDepartmentId, cancellationToken);

                if (currentRoom is null)
                {
                    throw new UnauthorizedAccessException("User cannot access this resource");
                }

                if (!IsSameRoom(currentRoom.Id, request.LockerId.Value))
                {
                    throw new UnauthorizedAccessException("User cannot access this resource");
                }
            }

            var logs = _context.LockerLogs
                .Include(x => x.ObjectId)
                .Include(x => x.User)
                .ThenInclude(x => x.Department)
                .AsQueryable();

            if (request.LockerId is not null)
            {
                logs = logs.Where(x => x.ObjectId! == request.LockerId);
            }

            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                logs = logs.Where(x => 
                    x.Action.Trim().ToLower().Contains(request.SearchTerm.Trim().ToLower()));
            }
            
            return await logs
                .LoggingListPaginateAsync<Locker, LockerLog, LockerLogDto>(
                    request.Page,
                    request.Size,
                    _mapper.ConfigurationProvider,
                    cancellationToken);
        }

        private async Task<Room?> GetRoomByDepartmentIdAsync(Guid departmentId, CancellationToken cancellationToken)
            => await _context.Rooms.FirstOrDefaultAsync(
            x => x.DepartmentId == departmentId,
            cancellationToken);

        private static bool IsSameRoom(Guid roomId1, Guid roomId2)
            => roomId1 == roomId2;
    }
}
