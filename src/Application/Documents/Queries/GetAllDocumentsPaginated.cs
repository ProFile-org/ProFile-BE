using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries;

public class GetAllDocumentsPaginated
{
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.RoomId)
                .Must((query, roomId) =>
                {
                    if (roomId is null)
                    {
                        return query.LockerId is null && query.FolderId is null;
                    }

                    if (query.LockerId is null)
                    {
                        return query.FolderId is null;
                    }

                    return true;
                }).WithMessage("Container orientation is not consistent");
        }
    }

    public record Query : IRequest<PaginatedList<DocumentDto>>
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

    public class QueryHandler : IRequestHandler<Query, PaginatedList<DocumentDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<DocumentDto>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var documents = _context.Documents.AsQueryable();
            var roomExists = request.RoomId is not null;
            var lockerExists = request.LockerId is not null;
            var folderExists = request.FolderId is not null;

            documents = documents
                .Include(x => x.Department)
                .Include(x => x.Folder)
                .ThenInclude(y => y.Locker)
                .ThenInclude(z => z.Room)
                .ThenInclude(t => t.Department);

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

                documents = documents
                    .Where(x => x.Folder!.Id == request.FolderId);
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

                documents = documents.Where(x => x.Folder!.Locker.Id == request.LockerId);
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

                documents = documents.Where(x => x.Folder!.Locker.Room.Id == request.RoomId);
            }

            var sortBy = request.SortBy;
            if (sortBy is null || !sortBy.MatchesPropertyName<LockerDto>())
            {
                sortBy = nameof(LockerDto.Id);
            }
            var sortOrder = request.SortOrder ?? "asc";
            var pageNumber = request.Page is null or <= 0 ? 1 : request.Page;
            var sizeNumber = request.Size is null or <= 0 ? 5 : request.Size;
            
            var list  = await documents
                .Paginate(pageNumber.Value, sizeNumber.Value)
                .OrderByCustom(sortBy, sortOrder)
                .ToListAsync(cancellationToken);
            
            var result = _mapper.Map<List<DocumentDto>>(list);

            return new PaginatedList<DocumentDto>(result, result.Count, pageNumber.Value, sizeNumber.Value);
        }
    }
}