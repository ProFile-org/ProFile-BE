using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities.Logging;
using Domain.Entities.Physical;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Org.BouncyCastle.Math.EC.Rfc8032;

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
        public Guid PerformingUserId { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public Guid RoomId { get; init; }
        public int Capacity { get; init; }
    }

    public class CommandHandler : IRequestHandler<Command, LockerDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
                throw new LimitExceededException(
                    "This room cannot accept more lockers."
                );
            }

            var locker = await _context.Lockers.FirstOrDefaultAsync(
                x => x.Name.Trim().ToLower().Equals(request.Name.Trim().ToLower()) && x.Room.Id.Equals(request.RoomId),
                cancellationToken);
            if (locker is not null)
            {
                throw new ConflictException("Locker name already exists.");
            }

            var performingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.PerformingUserId, cancellationToken);
            var entity = new Locker
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                NumberOfFolders = 0,
                Capacity = request.Capacity,
                Room = room,
                IsAvailable = true,
                Created = LocalDateTime.FromDateTime(DateTime.Now),
                CreatedBy = performingUser!.Id,
            };
            var log = new LockerLog()
            {
                User = performingUser,
                UserId = performingUser.Id,
                Object = entity,
                Time = LocalDateTime.FromDateTime(DateTime.Now),
                Action = LockerLogMessage.Add,
            };

            var result = await _context.Lockers.AddAsync(entity, cancellationToken);
            room.NumberOfLockers += 1;
            _context.Rooms.Update(room);
            await _context.LockerLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<LockerDto>(result.Entity);
        }
    }
}