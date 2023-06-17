using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities.Physical;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Folders.Queries;

public class GetAllFoldersPaginated
{
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.RoomId)
                .Must((query, roomId) => roomId is not null || query.LockerId is null);
        }
    }
    
    public record Query : IRequest<PaginatedList<FolderDto>>
    {
        public string CurrentUserRole { get; init; } = null!;
        public Guid CurrentUserDepartmentId { get; init; }
        public Guid? RoomId { get; init; }
        public Guid? LockerId { get; init; }
        public string? SearchTerm { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, PaginatedList<FolderDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<FolderDto>> Handle(Query request, CancellationToken cancellationToken)
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
            
            var folders = _context.Folders
                .Include(x => x.Locker)
                .ThenInclude(y => y.Room)
                .ThenInclude(z => z.Department)
                .AsQueryable();
            var roomIdProvided = request.RoomId is not null;
            var lockerIdProvided = request.LockerId is not null;

            if (lockerIdProvided)
            {
                var locker = await _context.Lockers
                    .Include(x => x.Room)
                    .FirstOrDefaultAsync(x => x.Id == request.LockerId
                                              && x.IsAvailable, cancellationToken);
                if (locker is null)
                {
                    throw new KeyNotFoundException("Locker does not exist.");
                }

                if (locker.Room.Id != request.RoomId)
                {
                    throw new ConflictException("Room does not match locker.");
                }

                folders = folders.Where(x => x.Locker.Id == request.LockerId);
            }
            else if (roomIdProvided)
            {
                var room = await _context.Rooms
                    .FirstOrDefaultAsync(x => x.Id == request.RoomId
                                              && x.IsAvailable, cancellationToken);
                if (room is null)
                {
                    throw new KeyNotFoundException("Room does not exist.");
                }

                folders = folders.Where(x => x.Locker.Room.Id == request.RoomId);
            }
            
            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                folders = folders.Where(x =>
                    x.Name.ToLower().Contains(request.SearchTerm.ToLower()));
            }

            return await folders
                .ListPaginateWithSortAsync<Folder, FolderDto>(
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