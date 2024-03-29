using Application.Common.Exceptions;
using Application.Common.Extensions.Logging;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AssignStaff> _logger;

        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<AssignStaff> logger)
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
                .FirstOrDefaultAsync(x => x.Id == request.StaffId, cancellationToken);
            
            if (staff is null)
            {
                throw new KeyNotFoundException("Staff does not exist.");
            }
            
            var room = await _context.Rooms
                .Include(x => x.Staff)
                .Include(x => x.Department)
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

            var user = await _context.Users
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == staff.Id, cancellationToken);

            if (user is null)
            {
                throw new KeyNotFoundException("User does not exist.");
            }

            if (room.Department.Id != user.Department!.Id)
            {
                throw new ConflictException("Room is in different department.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            staff.Room = room;
            room.Staff = staff;
            room.LastModified = localDateTimeNow;
            room.LastModifiedBy = request.CurrentUser.Id;
            
            _context.Rooms.Update(room);
            var result = _context.Staffs.Update(staff);
            await _context.SaveChangesAsync(cancellationToken);
            using (Logging.PushProperties(nameof(Staff), staff.Id, request.CurrentUser.Id))
            {
                _logger.LogAssignStaff(staff.Id.ToString(), staff.Room.Id.ToString());
            }
            return _mapper.Map<StaffDto>(result.Entity);
        }
    }
}