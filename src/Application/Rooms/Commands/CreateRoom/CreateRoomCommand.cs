using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities.Physical;
using MediatR;

namespace Application.Rooms.Commands.CreateRoom;

public record CreateRoomCommand : IRequest<RoomDto>
{
    public string Name { get; init; }
    public string Description { get; init; }
    public int Capacity { get; init; }
    
}

public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, RoomDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    public CreateRoomCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<RoomDto> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var entity = new Room
        {
            Name = request.Name,
            Description = request.Description,
            NumberOfLockers = 0,
            Capacity = request.Capacity
        };
        var result = await _context.Rooms.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<RoomDto>(result.Entity);
    }
}