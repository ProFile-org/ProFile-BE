using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Commands;

public class UpdateRoom
{
    public record Command : IRequest<RoomDto>
    {
        public Guid RoomId { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public int Capacity { get; init; }
    }
    
    public class CommandHandler : IRequestHandler<Command, RoomDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommandHandler(IApplicationDbContext context, IMapper mapper)
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
            
            var nameExisted = await _context.Rooms.AnyAsync(x => x.Name
                .ToLower().Equals(request.Name.ToLower())
                && x.Id != room.Id
                , cancellationToken: cancellationToken);

            if (nameExisted)
            {
                throw new ConflictException("New name has already exists.");
            }

            if (request.Capacity < room.NumberOfLockers)
            {
                throw new ConflictException("New capacity cannot be less than current number of lockers.");
            }

            var updatedRoom = new Room
            {
                Id = room.Id,
                Name = request.Name,
                Description = request.Description,
                Staff = room.Staff,
                Department = room.Department,
                DepartmentId = room.DepartmentId,
                Capacity = request.Capacity,
                NumberOfLockers = room.NumberOfLockers,
                IsAvailable = room.IsAvailable,
                Lockers = room.Lockers
            };

            _context.Rooms.Entry(room).State = EntityState.Detached;
            _context.Rooms.Entry(updatedRoom).State = EntityState.Modified;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return _mapper.Map<RoomDto>(updatedRoom);
        }
    }
}