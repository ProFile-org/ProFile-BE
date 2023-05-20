using Application.Common.Interfaces;
using Application.Users.Queries.Physical;
using AutoMapper;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Staffs.Commands.CreateStaff;

public record CreateStaffCommand : IRequest<StaffDto>
{
    public Guid UserId { get; init; }
    public Guid RoomId { get; init; }
}

public class CreateStaffCommandHandler : IRequestHandler<CreateStaffCommand, StaffDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    
    public CreateStaffCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<StaffDto> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
        if (user is null)
        {
            throw new KeyNotFoundException("User does not exist");
        }

        var room = await _context.Rooms.FirstOrDefaultAsync(x => x.Id == request.RoomId, cancellationToken);
        if (room is null)
        {
            throw new KeyNotFoundException("Room does not exist");
        }

        var staff = new Staff
        {
            Id = user.Id,
            User = user,
            Room = room
        };

        var result = await _context.Staffs.AddAsync(staff, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<StaffDto>(result.Entity);
    }
} 