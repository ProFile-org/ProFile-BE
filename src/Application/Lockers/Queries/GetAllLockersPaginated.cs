using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Lockers.Queries;

public class GetAllLockersPaginated
{
    public record Query : IRequest<PaginatedList<LockerDto>>
    {
        public string CurrentUserRole { get; init; } = null!;
        public Guid CurrentUserDepartmentId { get; init; }
        public Guid? RoomId { get; init; }
        public string? SearchTerm { get; init; }
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
            if (request.CurrentUserRole.IsStaff())
            {
                if (request.RoomId is null)
                {
                    throw new UnauthorizedAccessException("User cannot access this resource.");
                }
                
                var currentUserRoom = await GetRoomByDepartmentIdAsync(request.CurrentUserDepartmentId, cancellationToken);
                
                if (currentUserRoom is null)
                {
                    throw new UnauthorizedAccessException("User cannot access this resource.");
                }
                
                if (!IsSameRoom(currentUserRoom.Id, request.RoomId.Value))
                {
                    throw new UnauthorizedAccessException("User cannot access this resource.");
                }
            }
            
            var lockers = _context.Lockers
                .Include(x => x.Room)
                .ThenInclude(y => y.Department)
                .AsQueryable();

            if (request.RoomId is not null)
            {
                lockers = lockers.Where(x => x.Room.Id == request.RoomId);
            }
            
            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                lockers = lockers.Where(x =>
                    x.Name.ToLower().Contains(request.SearchTerm.ToLower()));
            }
            
            return await lockers
                .ListPaginateWithSortAsync<Locker, LockerDto>(
                    request.Page,
                    request.Size,
                    request.SortBy,
                    request.SortOrder,
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