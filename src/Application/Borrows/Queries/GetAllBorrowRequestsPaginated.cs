using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Borrows.Queries;

public class GetAllBorrowRequestsPaginated
{
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.RoomId)
                .Must((query, roomId) => roomId is null
                    ? query.LockerId is null && query.FolderId is null
                    : query.LockerId is not null || query.FolderId is null)
                .WithMessage("Container orientation is not consistent");
        }
    }
    
    public record Query : IRequest<PaginatedList<BorrowDto>>
    {
        public Guid? RoomId { get; init; }
        public Guid? LockerId { get; init; }
        public Guid? FolderId { get; init; }
        public string? SearchTerm { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
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
            var borrows = _context.Borrows.AsQueryable();
            var roomExists = request.RoomId is not null;
            var lockerExists = request.LockerId is not null;
            var folderExists = request.FolderId is not null;

            borrows = borrows
                .Include(x => x.Document)
                .ThenInclude(y => y.Department)
                .Include(x => x.Document)
                .ThenInclude(y => y.Folder)
                .ThenInclude(z => z!.Locker)
                .ThenInclude(t => t.Room)
                .ThenInclude(s => s.Department);

            if (folderExists)
            {
                var folder = await _context.Folders
                    .Include(x => x.Locker)
                    .ThenInclude(y => y.Room)
                    .FirstOrDefaultAsync(x => x.Id == request.FolderId
                                              && x.IsAvailable, cancellationToken);
                if (folder is null)
                {
                    throw new KeyNotFoundException("Folder does not exist.");
                }

                if (folder.Locker.Id != request.LockerId
                    || folder.Locker.Room.Id != request.RoomId)
                {
                    throw new ConflictException("Either locker or room does not match folder.");
                }

                borrows = borrows
                    .Where(x => x.Document.Folder!.Id == request.FolderId);
            }
            else if (lockerExists)
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

                borrows = borrows.Where(x => x.Document.Folder!.Locker.Id == request.LockerId);
            }
            else if (roomExists)
            {
                var room = await _context.Rooms
                    .FirstOrDefaultAsync(x => x.Id == request.RoomId
                                              && x.IsAvailable, cancellationToken);
                if (room is null)
                {
                    throw new KeyNotFoundException("Room does not exist.");
                }

                borrows = borrows.Where(x => x.Document.Folder!.Locker.Room.Id == request.RoomId);
            }

            var sortBy = request.SortBy;
            if (sortBy is null || !sortBy.MatchesPropertyName<BorrowDto>())
            {
                sortBy = nameof(BorrowDto.Id);
            }
            var sortOrder = request.SortOrder ?? "asc";
            var pageNumber = request.Page is null or <= 0 ? 1 : request.Page;
            var sizeNumber = request.Size is null or <= 0 ? 5 : request.Size;
            
            var list  = await borrows
                .Paginate(pageNumber.Value, sizeNumber.Value)
                .OrderByCustom(sortBy, sortOrder)
                .ToListAsync(cancellationToken);
            
            var result = _mapper.Map<List<BorrowDto>>(list);

            return new PaginatedList<BorrowDto>(result, result.Count, pageNumber.Value, sizeNumber.Value);
        }
    }
}