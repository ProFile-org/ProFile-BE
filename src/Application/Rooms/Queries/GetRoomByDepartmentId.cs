using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Queries;

public class GetRoomByDepartmentId
{
    public record Query : IRequest<RoomDto>
    {
        public Guid DepartmentId { get; init; }
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
                .Include(x => x.Department)
                .Include(x => x.Staff)
                .FirstOrDefaultAsync(x => x.DepartmentId == request.DepartmentId, cancellationToken: cancellationToken);
            
            if (room is null)
            {
                throw new KeyNotFoundException("Room does not exist.");
            }
            
            return _mapper.Map<RoomDto>(room);
        }
    }
}