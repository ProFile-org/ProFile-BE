using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Logging;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Staffs.Commands;

public class AddStaff
{
    public record Command : IRequest<StaffDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid StaffId { get; init; }
        public Guid? RoomId { get; init; }
    }

    public class CommandHandler : IRequestHandler<Command, StaffDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<StaffDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var staff = await _context.Staffs
                .FirstOrDefaultAsync(x => x.Id == request.StaffId, cancellationToken);
            
            if (staff is null)
            {
                throw new KeyNotFoundException("Staff does not exist.");
            }
            
            var room = await _context.Rooms
                .Include(x => x.Staff)
                .FirstOrDefaultAsync(x => x.Id == request.RoomId, cancellationToken);
            
            if (room is null)
            {
                throw new KeyNotFoundException("Room does not exist.");
            }
            
            if (room.Staff is not null)
            {
                throw new ConflictException("Room already has a staff.");
            }
            
            var log = new UserLog()
            {
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                Object = staff.User,
                Time = LocalDateTime.FromDateTime(DateTime.Now),
                Action = UserLogMessages.Staff.AssignStaff(room.Id.ToString()),
            };

            var result = await _context.Staffs.AddAsync(staff, cancellationToken);
            await _context.UserLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<StaffDto>(result.Entity);
            
        }
    }
}