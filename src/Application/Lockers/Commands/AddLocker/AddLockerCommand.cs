using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Physical;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Lockers.Commands.AddLocker;

public record AddLockerCommand : IRequest<LockerDto>
{
    public string Name { get; init; }
    public string Description { get; init; }
    public string RoomId { get; init; }
    public int Capacity { get; init; }
}

public class AddLockerCommandHandler : IRequestHandler<AddLockerCommand, LockerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AddLockerCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<LockerDto> Handle(AddLockerCommand request, CancellationToken cancellationToken)
    {
        var roomGuid = Guid.Parse(request.RoomId);
        var room = await _context.Rooms.FirstOrDefaultAsync(x => x.Id == roomGuid, cancellationToken);

        if (room is null)
        {
            throw new KeyNotFoundException("Room does not exist.");
        }

        if (room.NumberOfLockers == room.Capacity)
        {
            throw new LimitExceededException(
                "This room cannot accept more folders. Please remove a folder then try again."
                );
        }
        
        var entity = new Locker
        {
            Name = request.Name,
            Description = request.Description,
            NumberOfFolders = 0,
            Capacity = request.Capacity,
            Room = room
        };

        var result = await _context.Lockers.AddAsync(entity, cancellationToken);
        room.NumberOfLockers += 1;
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<LockerDto>(result.Entity);
    }
}