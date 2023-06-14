using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
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
        public Guid PerformingUserId { get; init; }
        public Guid UserId { get; init; }
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
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
            if (user is null)
            {
                throw new KeyNotFoundException("User does not exist.");
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

            var existedStaff = await _context.Staffs
                .Include(x => x.Room)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == user.Id, cancellationToken);
            var performingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.PerformingUserId, cancellationToken);
            if (existedStaff is not null)
            {
                if (existedStaff.Room is not null)
                {
                    throw new ConflictException("This user is already a staff.");
                }

                existedStaff.Room = room;
                var log = new UserLog()
                {
                    User = performingUser!,
                    UserId = performingUser!.Id,
                    Object = user,
                    Time = LocalDateTime.FromDateTime(DateTime.Now),
                    Action = UserLogMessages.Staff.AddStaff(room.Id.ToString()),
                };
                var result = _context.Staffs.Update(existedStaff);
                await _context.UserLogs.AddAsync(log, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return _mapper.Map<StaffDto>(result.Entity);
            }
            else
            {
                var staff = new Staff
                {
                    Id = user.Id,
                    User = user,
                    Room = room,
                };
                var log = new UserLog()
                {
                    User = performingUser!,
                    UserId = performingUser!.Id,
                    Object = user,
                    Time = LocalDateTime.FromDateTime(DateTime.Now),
                    Action = UserLogMessages.Staff.AddStaff(room.Id.ToString()),
                };

                var result = await _context.Staffs.AddAsync(staff, cancellationToken);
                await _context.UserLogs.AddAsync(log, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return _mapper.Map<StaffDto>(result.Entity);
            }
        }
    }
}