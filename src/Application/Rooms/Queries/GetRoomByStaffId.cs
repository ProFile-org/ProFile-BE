using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Queries;

public class GetRoomByStaffId
{
    public record Query : IRequest<RoomDto>
    {
        public Guid StaffId { get; init; }
    }
    
    public class QueryHandler : IRequestHandler<Query, RoomDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<RoomDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var room = await _context.Rooms
                .Include(x => x.Staff)
                .ThenInclude(y => y!.User)
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Staff!.Id == request.StaffId, cancellationToken);

            if (room is null)
            {
                throw new KeyNotFoundException("Room does not exists.");
            }

            return _mapper.Map<RoomDto>(room);
        }
    }
}