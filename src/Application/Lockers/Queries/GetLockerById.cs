using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Lockers.Queries;

public class GetLockerById
{
    public record Query : IRequest<LockerDto>
    {
        public string CurrentUserRole { get; init; } = null!;
        public Guid CurrentUserDepartmentId { get; init; }
        public Guid LockerId { get; init; }
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

            if (request.CurrentUserRole.IsStaff()
                && !LockerInSameDepartment(locker.Room.DepartmentId, request.CurrentUserDepartmentId))
            {
                throw new UnauthorizedAccessException();
            }
            
            return _mapper.Map<LockerDto>(locker);
        }

        private static bool LockerInSameDepartment(
            Guid lockerDepartmentId,
            Guid currentUserDepartmentId)
            => lockerDepartmentId == currentUserDepartmentId;
    }
}