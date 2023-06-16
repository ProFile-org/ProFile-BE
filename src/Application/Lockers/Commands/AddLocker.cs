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

public class AddLocker
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("Locker's capacity cannot be less than 1");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Locker's name is required.")
                .MaximumLength(64).WithMessage("Locker's name cannot exceed 64 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(256).WithMessage("Locker's description cannot exceed 256 characters.");

            RuleFor(x => x.RoomId)
                .NotEmpty().WithMessage("RoomId is required.");
        }
    }

    public record Command : IRequest<LockerDto>
    {
        public User CurrentUser { get; init; } = null!;
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public Guid RoomId { get; init; }
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
            var room = await _context.Rooms.FirstOrDefaultAsync(x => x.Id == request.RoomId, cancellationToken);

            if (room is null)
            {
                throw new KeyNotFoundException("Room does not exist.");
            }

            if (room.NumberOfLockers >= room.Capacity)
            {
                throw new LimitExceededException("This room cannot accept more lockers.");
            }

            if (await DuplicatedNameLockerExistsInSameRoomAsync(request.Name, request.RoomId, cancellationToken))
            {
                throw new ConflictException("Locker name already exists.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            var entity = new Locker()
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                NumberOfFolders = 0,
                Capacity = request.Capacity,
                Room = room,
                IsAvailable = true,
                Created = localDateTimeNow,
                CreatedBy = request.CurrentUser.Id,
            };
            
            var log = new LockerLog()
            {
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                Object = entity,
                Time = localDateTimeNow,
                Action = LockerLogMessage.Add,
            };
            var result = await _context.Lockers.AddAsync(entity, cancellationToken);
            room.NumberOfLockers += 1;
            _context.Rooms.Update(room);
            await _context.LockerLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<LockerDto>(result.Entity);
        }

        private async Task<bool> DuplicatedNameLockerExistsInSameRoomAsync(string lockerName, Guid roomId, CancellationToken cancellationToken)
        {
            var locker = await _context.Lockers.FirstOrDefaultAsync(
                x => EqualsInvariant(x.Name, lockerName) 
                     && IsSameRoom(x.Room.Id, roomId), cancellationToken);
            return locker is not null;
        }

        private static bool EqualsInvariant(string x, string y)
            => x.Trim().ToLower().Equals(y.Trim().ToLower());

        private static bool IsSameRoom(Guid roomId1, Guid roomId2)
            => roomId1 == roomId2;
    }
}