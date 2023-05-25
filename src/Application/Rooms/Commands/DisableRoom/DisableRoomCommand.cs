using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Commands.DisableRoom;

public record DisableRoomCommand : IRequest<RoomDto>
{
    public Guid RoomId { get; init; }
}

public class DisableRoomCommandHandler : IRequestHandler<DisableRoomCommand, RoomDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public DisableRoomCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<RoomDto> Handle(DisableRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await _context.Rooms
            .Include(x => x.Lockers)
            .ThenInclude(y => y.Folders)
            .FirstOrDefaultAsync(x => x.Id.Equals(request.RoomId), cancellationToken: cancellationToken);
        
        if (room is null)
        {
            throw new KeyNotFoundException("Room does not exist.");
        }

        if (!room.IsAvailable)
        {
            throw new InvalidOperationException("Room have already been disabled.");
        }

        var canNotDisable = await _context.Documents
                             .CountAsync(x => x.Folder!.Locker.Room.Id.Equals(request.RoomId), cancellationToken)
                             > 0;
        
        if (canNotDisable)
        {
            throw new InvalidOperationException("Room cannot be disabled because it contains documents.");
        }

        var lockers =  _context.Lockers.Include(x=> x.Folders)
            .Where(x => x.Room.Id.Equals(room.Id));
        
        foreach (var locker in lockers)
        {
            foreach (var folder in locker.Folders)
            {
                folder.IsAvailable = false;
            }
            locker.IsAvailable = false;
        }
        _context.Lockers.UpdateRange(lockers);
        room.IsAvailable = false;
        var result = _context.Rooms.Update(room);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<RoomDto>(result.Entity);
    }
}