using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Physical;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Lockers.Commands.AddLocker;

public record CreateLockerCommand : IRequest<LockerDto>
{
    public string Name { get; init; }
    public string Description { get; init; }
    public Guid RoomId { get; init; }
    public int Capacity { get; init; }
}

public class AddLockerCommandHandler : IRequestHandler<CreateLockerCommand, LockerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AddLockerCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<LockerDto> Handle(CreateLockerCommand request, CancellationToken cancellationToken)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(x => x.Id == request.RoomId, cancellationToken);

        if (room is null)
        {
            throw new KeyNotFoundException("Room does not exist.");
        }

        if (room.NumberOfLockers == room.Capacity)
        {
            throw new LimitExceededException(
                "This room cannot accept more lockers."
                );
        }

        var locker = await _context.Lockers.FirstOrDefaultAsync(x => x.Name.Equals(request.Name) && x.Room.Id.Equals(request.RoomId));
        if (locker is not null && locker.IsAvailable)
        {
            throw new ConflictException("Locker's name already exists");
        }
        if (locker is not null && !locker.IsAvailable)
        {
            locker.IsAvailable = true;
            room.NumberOfLockers += 1;
            var enabledresult = _context.Lockers.Update(locker);
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<LockerDto>(enabledresult.Entity);
        } 
        
        var entity = new Locker
        {
            Name = request.Name,
            Description = request.Description,
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