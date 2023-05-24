using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries.GetAllDocumentsPaginated;

public record GetAllDocumentsPaginatedQuery : IRequest<PaginatedList<DocumentItemDto>>
{
    public Guid? RoomId { get; init; }
    public Guid? LockerId { get; init; }
    public Guid? FolderId { get; init; }
    public int? Page { get; init; }
    public int? Size { get; init; }
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; }
}

public class GetAllDocumentsPaginatedQueryHandler : IRequestHandler<GetAllDocumentsPaginatedQuery, PaginatedList<DocumentItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllDocumentsPaginatedQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<DocumentItemDto>> Handle(GetAllDocumentsPaginatedQuery request,
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

        var sortBy = request.SortBy ?? nameof(DocumentItemDto.Id);
        var sortOrder = request.SortOrder ?? "asc";
        var pageNumber = request.Page ?? 1;
        var sizeNumber = request.Size ?? 5;
        var result = await documents
            .ProjectTo<DocumentItemDto>(_mapper.ConfigurationProvider)
            .OrderByCustom(sortBy, sortOrder)
            .PaginatedListAsync(pageNumber, sizeNumber);

        return result;
    }
} 