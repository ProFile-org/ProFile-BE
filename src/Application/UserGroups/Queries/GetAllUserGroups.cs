using System.Collections.ObjectModel;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Digital;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UserGroups.Queries;

public class GetAllUserGroups
{
    public record Query : IRequest<IEnumerable<UserGroupDto>>;

    public class QueryHandler : IRequestHandler<Query, IEnumerable<UserGroupDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserGroupDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userGroups = await _context.UserGroups
                .ToListAsync(cancellationToken);
            var result = new ReadOnlyCollection<UserGroupDto>(_mapper.Map<List<UserGroupDto>>(userGroups));
            return result;
        }
    } 
}