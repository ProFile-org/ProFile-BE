using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Identity;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Users.Queries;

public class GetAllUsersPaginated
{
    public record Query : IRequest<PaginatedList<UserDto>>
    {
        public Guid[]? DepartmentIds { get; init; }
        public string? Role { get; init; }
        public string? SearchTerm { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, PaginatedList<UserDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<UserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var users = _context.Users
                .Where(x => !x.Role.Equals(IdentityData.Roles.Admin));

            // Filter by department
            if (request.DepartmentIds is not null)
            {
                users = users.Where(x => request.DepartmentIds.Contains(x.Department!.Id) );
            }

            // Filter by role
            if (request.Role is not null)
            {
                users = users.Where(x => x.Role.Equals(request.Role));
            }
            
            // Search
            if (!(request.SearchTerm is null || request.SearchTerm.Trim().Equals(string.Empty)))
            {
                users = users.Where(x =>
                    x.FirstName!.ToLower().Contains(request.SearchTerm.Trim().ToLower()));
            }
            
            return await users
                .ListPaginateWithFilterAsync<User, UserDto>(
                    request.Page,
                    request.Size,
                    request.SortBy,
                    request.SortOrder,
                    _mapper.ConfigurationProvider,
                    cancellationToken);
        }
    }
}