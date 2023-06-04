using Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Queries;

public class GetUserById
{
    public record Query : IRequest<UserDto>
    {
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

            return _mapper.Map<UserDto>(user);
        }
    }
}