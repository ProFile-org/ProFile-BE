using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.ImportDocument;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ImportRequests.Queries;

public class GetAllImportRequestsPaginated
{
    public record Query : IRequest<PaginatedList<ImportRequestDto>>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid? RoomId { get; init; }
        public string? SearchTerm { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, PaginatedList<ImportRequestDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<ImportRequestDto>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            if (request.CurrentUser.Role.IsStaff()
                && request.RoomId is null)
            {
                throw new UnauthorizedAccessException("User can not access this resource.");
            }
            
            if (request.CurrentUser.Role.IsEmployee())
            {
                var room = await _context.Rooms
                    .FirstOrDefaultAsync(x => x.Id == request.RoomId, cancellationToken);
                var roomDoesNotExist = room is null;
                
                if (roomDoesNotExist
                    || RoomIsNotInSameDepartment(request.CurrentUser, room!))
                {
                    throw new UnauthorizedAccessException("User can not access this resource.");
                }
            }
            
            var importRequests = _context.ImportRequests
                .Include(x => x.Document)
                .Include(x => x.Room)
                .ThenInclude(x => x.Department)
                .AsQueryable();
            
            if (request.RoomId is not null)
            {
                importRequests = importRequests.Where(x => x.RoomId == request.RoomId);
            }
            
            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                importRequests = importRequests.Where(x =>
                    x.Document.Title.ToLower().Contains(request.SearchTerm.ToLower()));
            }
            
            
            return await importRequests
                .ListPaginateWithSortAsync<ImportRequest, ImportRequestDto>(
                request.Page,
                request.Size,
                request.SortBy,
                request.SortOrder,
                _mapper.ConfigurationProvider,
                cancellationToken);
        }
        
        
        private static bool RoomIsNotInSameDepartment(User user, Room room)
            => user.Department?.Id != room.DepartmentId;
    }
}