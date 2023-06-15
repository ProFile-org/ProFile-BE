using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Queries;

public class GetAllUsersPaginated
{
    public record Query : IRequest<PaginatedList<UserDto>>
    {
        public Guid? DepartmentId { get; init; }
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
            var users = _context.Users.AsQueryable()
                .Where(x => !x.Role.Equals(IdentityData.Roles.Admin));

            if (request.DepartmentId is not null)
            {
                users = users.Where(x => x.Department!.Id == request.DepartmentId);
            }
            
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