using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Identity;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Queries;

public class GetAllEmployeesPaginated
{
    public record Query : IRequest<PaginatedList<UserDto>>
    {
        public Guid DepartmentId { get; init; }
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? SortBy { get; init; }
        public string? SortOrder { get; init; }
    }

    public class Handler : IRequestHandler<Query, PaginatedList<UserDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Handler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<UserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var users = _context.Users
                .Include(x => x.Department)
                .Where(x => x.Department!.Id == request.DepartmentId
                            && x.Role.Equals(IdentityData.Roles.Employee)
                            && x.IsActive
                            && x.IsActivated);
            
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
