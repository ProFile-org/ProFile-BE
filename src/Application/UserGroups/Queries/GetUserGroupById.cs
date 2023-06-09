using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Digital;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UserGroups.Queries;

public class GetUserGroupById
{
    public record Query : IRequest<UserGroupDto>
    {
        public Guid UserGroupId { get; set; }
    }

    public class QueryHandler : IRequestHandler<Query, UserGroupDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<UserGroupDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var userGroup = await _context.UserGroups
                .FirstOrDefaultAsync(
                    x => x.Id == request.UserGroupId
                    , cancellationToken);

            if (userGroup is null)
            {
                throw new KeyNotFoundException("User group does not exist.");
            }

            return _mapper.Map<UserGroupDto>(userGroup);
        }
    }
}