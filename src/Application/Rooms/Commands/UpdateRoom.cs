using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Logging;
using Domain.Entities.Physical;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Rooms.Commands;

public class UpdateRoom
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name can not be empty.")
                .MaximumLength(64).WithMessage("Name can not exceed 64 characters.");

            RuleFor(x => x.Capacity)
                .NotEmpty().WithMessage("Capacity can not be empty.");

            RuleFor(x => x.Description)
                .MaximumLength(256).WithMessage("Description can not exceed 256 characters.");
        }
    }
    public record Command : IRequest<RoomDto>
    {
        public User CurrentUser { get; init; } = null!;
        public Guid RoomId { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public int Capacity { get; init; }
        public bool IsAvailable { get; init; }
    }
    
    public class CommandHandler : IRequestHandler<Command, RoomDto>
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

        public async Task<RoomDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var room = await _context.Rooms
                .Include(x => x.Department)
                .Include(x => x.Staff)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id.Equals(request.RoomId), cancellationToken: cancellationToken);
          
            if (room is null)
            {
                throw new KeyNotFoundException("Room does not exist.");
            }
            
            if (await DuplicatedNameRoomExistsAsync(request.Name, request.RoomId, cancellationToken))
            {
                throw new ConflictException("Name has already exists.");
            }

            if (request.Capacity < room.NumberOfLockers)
            {
                throw new ConflictException("New capacity cannot be less than current number of lockers.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            // update work
            room.Name = request.Name;
            room.Description = request.Description;
            room.Capacity = request.Capacity;
            room.IsAvailable = request.IsAvailable;
            room.LastModified = localDateTimeNow;
            room.LastModifiedBy = request.CurrentUser.Id;
            
            var log = new RoomLog()
            {
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                ObjectId = room.Id,
                Time = localDateTimeNow,
                Action = RoomLogMessage.Update,
            };
            _context.Rooms.Entry(room).State = EntityState.Modified;
            await _context.RoomLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<RoomDto>(room);
        }
        
        private async Task<bool> DuplicatedNameRoomExistsAsync(
            string roomName,
            Guid roomId,
            CancellationToken cancellationToken)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(
                x => x.Name.Trim().ToLower().Equals(roomName.Trim().ToLower())
                     && x.Id != roomId,
                cancellationToken);
            return room is not null;
        }
    }
}