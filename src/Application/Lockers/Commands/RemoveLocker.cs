using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Logging;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Lockers.Commands;

public class RemoveLocker
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
        
            RuleFor(x => x.LockerId)
                .NotEmpty().WithMessage("LockerId is required.");
        }
    }

    public record Command : IRequest<LockerDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid LockerId { get; init; }
    }
    
    public class CommandHandler : IRequestHandler<Command, LockerDto>
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
    
        public async Task<LockerDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var locker = await _context.Lockers
                .Include(x => x.Room)
                .ThenInclude(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.LockerId), cancellationToken);
            
            if (locker is null)
            {
                throw new KeyNotFoundException("Locker does not exist.");
            }

            var canNotRemove = await _context.Documents
                .AnyAsync(x => x.Folder!.Locker.Id.Equals(request.LockerId), cancellationToken);
            if (canNotRemove)
            {
                throw new ConflictException("Locker cannot be removed because it contains documents.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            var log = new LockerLog()
            {
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                ObjectId = locker.Id,
                Time = localDateTimeNow,
                Action = LockerLogMessage.Remove,
            };
            var room = locker.Room;
            var result = _context.Lockers.Remove(locker);
            room.NumberOfLockers -= 1;
            _context.Rooms.Update(room);
            await _context.LockerLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<LockerDto>(result.Entity);
        }
    }
}