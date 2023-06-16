using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Identity;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Queries;

public class GetUserById
{
    public record Query : IRequest<UserDto>
    {
        public string UserRole { get; init; } = null!;
        public Guid UserDepartmentId { get; init; }
        public Guid UserId { get; init; }
    }

    public class QueryHandler : IRequestHandler<Query, UserDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<UserDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.UserId), cancellationToken: cancellationToken);

            if (user is null)
            {
                throw new KeyNotFoundException("User does not exist.");
            }

            if (ViolateConstraints(request.UserRole, request.UserDepartmentId, user))
            {
                throw new UnauthorizedAccessException("User cannot access this resource.");
            }

            return _mapper.Map<UserDto>(user);
        }

        private static bool ViolateConstraints(string userRole, Guid userDepartmentId, User foundUser)
            => (userRole.IsStaff() || userRole.IsEmployee()) 
               && GetUserInOtherDepartment(userDepartmentId, foundUser);

        private static bool GetUserInOtherDepartment(Guid userDepartmentId, User foundUser)
            => foundUser.Department?.Id != userDepartmentId;
    }
}