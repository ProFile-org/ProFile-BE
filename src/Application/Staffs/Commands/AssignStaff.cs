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

public class AssignStaff
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
        private readonly IDateTimeProvider _dateTimeProvider;

        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<StaffDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var staff = await _context.Staffs
                .Include(x => x.User)
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
            
            if (!room.IsAvailable)
            {
                throw new ConflictException("Room is not available.");
            }
            
            if (room.Staff is not null)
            {
                throw new ConflictException("Room already has a staff.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            staff.Room = room;
            room.Staff = staff;
            room.LastModified = localDateTimeNow;
            room.LastModifiedBy = request.CurrentUser.Id;
            
            var log = new UserLog()
            {
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                ObjectId = staff.User.Id,
                Time = localDateTimeNow,
                Action = UserLogMessages.Staff.AssignStaff(room.Id.ToString()),
            };
            _context.Rooms.Update(room);
            var result = _context.Staffs.Update(staff);
            await _context.UserLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<StaffDto>(result.Entity);
            
        }
    }
}