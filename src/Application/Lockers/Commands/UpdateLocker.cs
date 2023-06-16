using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Logging;
using Domain.Entities.Physical;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Lockers.Commands;

public class UpdateLocker
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(64).WithMessage("Locker's name cannot exceed 64 characters.");
            
            RuleFor(x => x.Description)
                .MaximumLength(256).WithMessage("Locker's description cannot exceed 256 characters.");
            
            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("Locker's capacity cannot be less than 1");
        }
    }
    public record Command : IRequest<LockerDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid LockerId { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public int Capacity { get; init; }
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
            
            if (await DuplicatedNameLockerExistsInSameRoomAsync(request.Name, locker.Room.Id, request.LockerId, cancellationToken))
            {
                throw new ConflictException("New locker name already exists.");
            }
            
            if (locker.NumberOfFolders > request.Capacity)
            {
                throw new ConflictException("New capacity cannot be less than current number of folders.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);
            
            // update work
            locker.Name = request.Name;
            locker.Description = request.Description;
            locker.Capacity = request.Capacity;
            locker.LastModified = localDateTimeNow;
            locker.LastModifiedBy = request.CurrentUser.Id;
            
            var log = new LockerLog()
            {
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                Object = locker,
                Time = localDateTimeNow,
                Action = LockerLogMessage.Update,
            };
            var result = _context.Lockers.Update(locker);
            await _context.LockerLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<LockerDto>(result.Entity);
        }
        
        private async Task<bool> DuplicatedNameLockerExistsInSameRoomAsync(
            string lockerName,
            Guid roomId,
            Guid lockerId,
            CancellationToken cancellationToken)
        {
            var locker = await _context.Lockers.FirstOrDefaultAsync(
                x => EqualsInvariant(x.Name, lockerName) 
                     && IsNotSameLocker(x.Id, lockerId)
                     && IsSameRoom(x.Room.Id, roomId),
                cancellationToken);
            return locker is not null;
        }
        
        private static bool EqualsInvariant(string x, string y)
            => x.Trim().ToLower().Equals(y.Trim().ToLower());

        private static bool IsSameRoom(Guid roomId1, Guid roomId2)
            => roomId1 == roomId2;

        private static bool IsNotSameLocker(Guid lockerId1, Guid lockerId2)
            => lockerId1 != lockerId2;
    }
}