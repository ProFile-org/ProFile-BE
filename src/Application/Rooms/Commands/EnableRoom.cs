using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Commands;

public class EnableRoom
{
    public record Command : IRequest<RoomDto>
    {
        public Guid RoomId { get; init; }
    }

    public class EnableRoomCommandHandler : IRequestHandler<Command, RoomDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public EnableRoomCommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<RoomDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(x => x.Id.Equals(request.RoomId), cancellationToken: cancellationToken);

            if (room is null)
            {
                throw new KeyNotFoundException("Room does not exist.");
            }

            if (room.IsAvailable)
            {
                throw new ConflictException("Room have already been enabled.");
            }

            room.IsAvailable = true;
            var result = _context.Rooms.Update(room);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<RoomDto>(result.Entity);
        }
    }
}