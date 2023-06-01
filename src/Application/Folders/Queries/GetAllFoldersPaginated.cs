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
            var folders = _context.Folders.AsQueryable();
            var roomExists = request.RoomId is not null;
            var lockerExists = request.LockerId is not null;

            if (lockerExists)
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
            else if (roomExists)
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

            var sortBy = request.SortBy;
            if (sortBy is null || !sortBy.MatchesPropertyName<LockerDto>())
            {
                sortBy = nameof(LockerDto.Id);
            }
            var sortOrder = request.SortOrder ?? "asc";
            var pageNumber = request.Page is null or <= 0 ? 1 : request.Page;
            var sizeNumber = request.Size is null or <= 0 ? 5 : request.Size;

            var result = await folders
                .ProjectTo<FolderDto>(_mapper.ConfigurationProvider)
                .OrderByCustom(sortBy, sortOrder)
                .PaginatedListAsync(pageNumber.Value, sizeNumber.Value);

            return result;
        }
    }
}