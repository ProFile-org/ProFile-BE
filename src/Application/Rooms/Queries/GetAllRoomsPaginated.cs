using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Queries;

public class GetAllRoomsPaginated
{
    public record Query : IRequest<PaginatedList<RoomDto>>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid? DepartmentId { get; init; }
        public string? SearchTerm { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, PaginatedList<RoomDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<RoomDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var rooms = _context.Rooms
                .Include(x => x.Department)
                .Include(x => x.Staff)
                .AsQueryable();

            if (request.CurrentUser.Role.IsEmployee()
                && request.CurrentUser.Department?.Id != request.DepartmentId)
            {
                throw new UnauthorizedAccessException("User cannot access this resource.");
            }
            
            rooms = rooms.Where(x => x.Department.Id == request.DepartmentId);

            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                rooms = rooms.Where(x =>
                    x.Name.ToLower().Contains(request.SearchTerm.ToLower()));
            }
            
            return await rooms
                .ListPaginateWithSortAsync<Room, RoomDto>(
                    request.Page,
                    request.Size,
                    request.SortBy,
                    request.SortOrder,
                    _mapper.ConfigurationProvider,
                    cancellationToken);
        }
    }
}