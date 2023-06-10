using Application.AuthorizationRequirements;
using Application.Common.AbstractClasses;
using Application.Common.AccessControl.Models;
using Application.Common.AccessControl.Models.Operations;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Lockers.Queries;

public class GetLockerById
{
    public record Query : IRequest<LockerDto>
    {
        public Guid LockerId { get; init; }
    }

    public class Authorizer : AbstractRequestAuthorizer<Query>
    {
        public override void BuildPolicy(Query request)
        {
            UseRequirement(new MustBeGrantedRequirement()
            {
                Resource = new PhysicalResource()
                {
                    Id = request.LockerId,
                    Type = ResourceType.Locker,
                },
                
                Operation = LockerOperation.Read
            });
        }
    }
    public class QueryHandler : IRequestHandler<Query, LockerDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<LockerDto> Handle(Query request, CancellationToken cancellationToken)
        {
            
            var locker = await _context.Lockers
                .Include(x => x.Room)
                .ThenInclude(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.LockerId), cancellationToken);

            if (locker is null)
            {
                throw new KeyNotFoundException("Locker does not exist.");
            }

            return _mapper.Map<LockerDto>(locker);
        }
    }
}