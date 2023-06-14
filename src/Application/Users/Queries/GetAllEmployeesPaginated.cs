using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Identity;
using AutoMapper;
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
            var users = _context.Users.AsQueryable()
                .Include(x => x.Department)
                .Where(x => x.Department!.Id == request.DepartmentId 
                            && x.Role.Equals(IdentityData.Roles.Employee)
                            && x.IsActive
                            && x.IsActivated);

            var sortBy = request.SortBy;
            if (sortBy is null || !sortBy.MatchesPropertyName<UserDto>())
            {
                sortBy = nameof(UserDto.Id);
            }
            var sortOrder = request.SortOrder ?? "asc";
            var pageNumber = request.Page is null or <= 0 ? 1 : request.Page;
            var sizeNumber = request.Size is null or <= 0 ? 5 : request.Size;
            
            var count = await users.CountAsync(cancellationToken);
            var list  = await users
                .Paginate(pageNumber.Value, sizeNumber.Value)
                .OrderByCustom(sortBy, sortOrder)
                .ToListAsync(cancellationToken);
            
            var result = _mapper.Map<List<UserDto>>(list);

            return new PaginatedList<UserDto>(result, count, pageNumber.Value, sizeNumber.Value);
        }
    }
}
