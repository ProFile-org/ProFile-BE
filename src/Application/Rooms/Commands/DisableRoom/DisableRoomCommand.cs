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
            .FirstOrDefaultAsync(x => x.Id.Equals(request.RoomId) && x.IsAvailable == true, cancellationToken: cancellationToken);
        if (room is null)
        {
            throw new KeyNotFoundException("Room does not exist");
        }
        
        var canDisable = await _context.Documents.CountAsync(x => x.Folder!.Locker.Room.Id.Equals(request.RoomId), cancellationToken: cancellationToken);
        
        if (canDisable > 0)
        {
                throw new InvalidOperationException("Room cannot be disabled because it contains documents");
        }
        
        room.IsAvailable = false;
        var result = _context.Rooms.Update(room);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<RoomDto>(result.Entity);
    }
}