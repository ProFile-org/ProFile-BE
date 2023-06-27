using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Physical;
using Domain.Statuses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Borrows.Queries;

public class GetAllBorrowRequestsPaginated
{
    public record Query : IRequest<PaginatedList<BorrowDto>>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid? RoomId { get; init; }
        public Guid? DocumentId { get; init; }
        public Guid? EmployeeId { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
        public string[]? Statuses { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, PaginatedList<BorrowDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<BorrowDto>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            if (request.CurrentUser.Role.IsStaff())
            {
                if (request.RoomId is null)
                {
                    throw new UnauthorizedAccessException("User can not access this resource.");
                }

                var room = await _context.Rooms
                    .FirstOrDefaultAsync(x => x.Id == request.RoomId, cancellationToken);
                var roomDoesNotExist = room is null;

                if (roomDoesNotExist
                    || RoomIsNotInSameDepartment(request.CurrentUser, room!))
                {
                    throw new UnauthorizedAccessException("User can not access this resource.");
                }
            }

            if (request.CurrentUser.Role.IsEmployee())
            {
                if (request.RoomId is null)
                {
                    throw new UnauthorizedAccessException("User can not access this resource.");
                }

                if (request.EmployeeId != request.CurrentUser.Id)
                {
                    throw new UnauthorizedAccessException("User can not access this resource");
                }

                var room = await _context.Rooms
                    .FirstOrDefaultAsync(x => x.Id == request.RoomId, cancellationToken);
                var roomDoesNotExist = room is null;

                if (roomDoesNotExist
                    || RoomIsNotInSameDepartment(request.CurrentUser, room!))
                {
                    throw new UnauthorizedAccessException("User can not access this resource.");
                }
            }

            var borrows = _context.Borrows.AsQueryable();

            borrows = borrows
                .Include(x => x.Borrower)
                .Include(x => x.Document)
                .ThenInclude(y => y.Department)
                .Include(x => x.Document)
                .ThenInclude(y => y.Folder)
                .ThenInclude(z => z!.Locker)
                .ThenInclude(t => t.Room)
                .ThenInclude(s => s.Department);

            if (request.RoomId is not null)
            {
                borrows = borrows.Where(x => x.Document.Folder!.Locker.Room.Id == request.RoomId);
            }
            
            if (request.EmployeeId is not null)
            {
                borrows = borrows.Where(x => x.Borrower.Id == request.EmployeeId);
            }
            
            if (request.DocumentId is not null)
            {
                borrows = borrows.Where(x => x.Document.Id == request.DocumentId);
            }

            if (request.Statuses is not null)
            {
                var statuses = request.Statuses.Aggregate(new List<BorrowRequestStatus>(),
                    (statuses, currentStatus) =>
                    {
                        if (!Enum.TryParse<BorrowRequestStatus>(currentStatus, true, out var validStatus)) return statuses;
                        
                        statuses.Add(validStatus);
                        return statuses;
                    });
                borrows = borrows.Where(x => statuses.Contains(x.Status));
            }
            
            var sortBy = request.SortBy;
            if (sortBy is null || !sortBy.MatchesPropertyName<BorrowDto>())
            {
                sortBy = nameof(BorrowDto.Status);
            }
            var sortOrder = request.SortOrder ?? "asc";
            var pageNumber = request.Page is null or <= 0 ? 1 : request.Page;
            var sizeNumber = request.Size is null or <= 0 ? 5 : request.Size;

            var count = await borrows.CountAsync(cancellationToken);
            var list  = await borrows
                .OrderByCustom(sortBy, sortOrder)
                .Paginate(pageNumber.Value, sizeNumber.Value)
                .ToListAsync(cancellationToken);

            var result = _mapper.Map<List<BorrowDto>>(list);

            return new PaginatedList<BorrowDto>(result, count, pageNumber.Value, sizeNumber.Value);
        }

        private static bool RoomIsNotInSameDepartment(User user, Room room)
            => user.Department?.Id != room.DepartmentId;
    }
}