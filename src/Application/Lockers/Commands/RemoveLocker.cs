using Application.Common.Exceptions;
using Application.Common.Extensions.Logging;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Physical;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<RemoveLocker> _logger;

        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider, ILogger<RemoveLocker> logger)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
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

            var room = locker.Room;
            var result = _context.Lockers.Remove(locker);
            room.NumberOfLockers -= 1;
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync(cancellationToken);
            using (Logging.PushProperties(nameof(Locker), locker.Id, request.CurrentUser.Id))
            {
                _logger.LogRemoveLocker(locker.Id.ToString(), locker.Room.Id.ToString(), locker.Room.Department.Name);
            }
            return _mapper.Map<LockerDto>(result.Entity);
        }
    }
}