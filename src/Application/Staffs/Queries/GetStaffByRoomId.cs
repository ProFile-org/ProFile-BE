using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Staffs.Queries;

public class GetStaffByRoomId
{
    public record Query : IRequest<StaffDto>
    {
        public Guid RoomId { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, StaffDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<StaffDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var room = await _context.Rooms
                .Include(x => x.Department)
                .Include(x => x.Staff)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.RoomId), cancellationToken: cancellationToken);

            if (room is null)
            {
                throw new KeyNotFoundException("Room does not exist.");
            }

            if (room.Staff is null)
            {
                throw new KeyNotFoundException("Staff does not exist.");
            }
            
            return _mapper.Map<StaffDto>(room.Staff);
        }
    }
}