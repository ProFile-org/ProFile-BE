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
                .Must((query, roomId) => roomId is null
                    ? query.LockerId is null && query.FolderId is null
                    : query.LockerId is not null || query.FolderId is null).WithMessage("Container orientation is not consistent");
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

            documents = documents.Include(x => x.Department);

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
            
            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                documents = documents.Where(x =>
                    x.Title.ToLower().Contains(request.SearchTerm.ToLower()));
            }

            var sortBy = request.SortBy ?? nameof(DocumentDto.Id);
            var sortOrder = request.SortOrder ?? "asc";
            var pageNumber = request.Page ?? 1;
            var sizeNumber = request.Size ?? 5;
            var result = await documents
                .ProjectTo<DocumentDto>(_mapper.ConfigurationProvider)
                .OrderByCustom(sortBy, sortOrder)
                .PaginatedListAsync(pageNumber, sizeNumber);

            return result;
        }
    }
}