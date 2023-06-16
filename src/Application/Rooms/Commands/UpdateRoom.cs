using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
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
        public Guid PerformingUserId { get; init; }
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

        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<RoomDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var room = await _context.Rooms
                .Include(x => x.Department)
                .Include(x => x.Staff)
                .FirstOrDefaultAsync(x => x.Id.Equals(request.RoomId), cancellationToken: cancellationToken);
          
            if (room is null)
            {
                throw new KeyNotFoundException("Room does not exist.");
            }
            
            var nameExisted = await _context.Rooms.AnyAsync(x => x.Name
                .ToLower().Equals(request.Name.ToLower())
                && x.Id != room.Id
                , cancellationToken: cancellationToken);

            if (nameExisted)
            {
                throw new ConflictException("Name has already exists.");
            }

            if (request.Capacity < room.NumberOfLockers)
            {
                throw new ConflictException("New capacity cannot be less than current number of lockers.");
            }

            var performingUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == request.PerformingUserId, cancellationToken);
            
            // update work
            room.Name = request.Name;
            room.Description = request.Description;
            room.Capacity = request.Capacity;
            room.IsAvailable = request.IsAvailable;
            room.LastModified = LocalDateTime.FromDateTime(DateTime.Now);
            room.LastModifiedBy = performingUser!.Id;
            
            var log = new RoomLog()
            {
                User = performingUser,
                UserId = performingUser.Id,
                Object = room,
                Time = LocalDateTime.FromDateTime(DateTime.Now),
                Action = RoomLogMessage.Update,
            };

            var result = _context.Rooms.Update(room);
            await _context.RoomLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<RoomDto>(result.Entity);
        }
    }
}