using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Commands.Add;

public record Command : IRequest<RoomDto>
{
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

        var room = await _context.Rooms.FirstOrDefaultAsync(r =>
            r.Name.Trim().ToLower().Equals(request.Name.Trim().ToLower()), cancellationToken);

        if (room is not null)
        {
            throw new ConflictException("Room name already exists.");
        }
        
        var entity = new Room
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            NumberOfLockers = 0,
            Capacity = request.Capacity
        };
        var result = await _context.Rooms.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<RoomDto>(result.Entity);
    }
}