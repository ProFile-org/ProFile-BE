using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Staffs.Commands;

public class RemoveStaffFromRoom
{
    public record Command : IRequest<StaffDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid StaffId { get; init; }
    }
    
    public class CommandHandler : IRequestHandler<Command, StaffDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<RemoveStaffFromRoom> _logger;

        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<RemoveStaffFromRoom> logger)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<StaffDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var staff = await _context.Staffs
                .Include(x => x.User)
                .Include(x => x.Room)
                .FirstOrDefaultAsync(x => x.User.Id.Equals(request.StaffId), cancellationToken: cancellationToken);

            if (staff is null)
            {
                throw new KeyNotFoundException("Staff does not exist.");
            }

            if (staff.Room is null)
            {
                throw new ConflictException("Staff is not assigned to a room.");
            }

            var room = staff.Room;

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);
            
            staff.Room.Staff = null;
            _context.Rooms.Update(staff.Room!);
            staff.Room = null;
            
            var log = new UserLog()
            {
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                ObjectId = staff.User.Id,
                Time = localDateTimeNow,
                Action = UserLogMessages.Staff.RemoveFromRoom,
            };
            var result = _context.Staffs.Update(staff);
            await _context.UserLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            using (Logging.PushProperties(nameof(User), result.Entity.Id, request.CurrentUser.Id))
            {
                _logger.LogRemoveStaffFromRoom(result.Entity.Id.ToString(), room.Id.ToString());
            }
            return _mapper.Map<StaffDto>(result.Entity);
        }
    }
}