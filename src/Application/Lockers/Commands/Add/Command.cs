using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Physical;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Lockers.Commands.Add;

public record Command : IRequest<LockerDto>
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public Guid RoomId { get; init; }
    public int Capacity { get; init; }
}

public class CommandHandler : IRequestHandler<Command, LockerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<LockerDto> Handle(Command request, CancellationToken cancellationToken)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(x => x.Id == request.RoomId, cancellationToken);

        if (room is null)
        {
            throw new KeyNotFoundException("Room does not exist.");
        }

        if (room.NumberOfLockers >= room.Capacity)
        {
            throw new LimitExceededException(
                "This room cannot accept more lockers."
                );
        }

        var locker = await _context.Lockers.FirstOrDefaultAsync(x => x.Name.Trim().ToLower().Equals(request.Name.Trim().ToLower()) && x.Room.Id.Equals(request.RoomId) ,cancellationToken);
        if (locker is not null)
        {
            throw new ConflictException("Locker name already exists.");
        }
        
        var entity = new Locker
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            NumberOfFolders = 0,
            Capacity = request.Capacity,
            Room = room,
            IsAvailable = true
        };

        var result = await _context.Lockers.AddAsync(entity, cancellationToken);
        room.NumberOfLockers += 1;
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<LockerDto>(result.Entity);
    }
}