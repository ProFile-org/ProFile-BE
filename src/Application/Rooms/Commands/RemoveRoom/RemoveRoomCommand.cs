using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Commands.RemoveRoom;

public record RemoveRoomCommand : IRequest<RoomDto>
{
    public Guid RoomId { get; init; }    
}

public class RemoveRoomCommandHandler : IRequestHandler<RemoveRoomCommand, RoomDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public RemoveRoomCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<RoomDto> Handle(RemoveRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await _context.Rooms
            .FirstOrDefaultAsync(x => x.Id.Equals(request.RoomId), cancellationToken: cancellationToken);

        if (room is null)
        {
            throw new KeyNotFoundException("Room does not exist");
        }

        var canRemove = await _context.Documents
            .CountAsync(x => x.Folder!.Locker.Room.Id.Equals(request.RoomId), cancellationToken: cancellationToken);
        if (canRemove > 0)
        {
            throw new InvalidOperationException("Can not remove room when room still have documents");
        }

        room.IsAvailable = false;
        var result = _context.Rooms.Remove(room);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<RoomDto>(result.Entity);
    }
}