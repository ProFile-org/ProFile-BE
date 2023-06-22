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

public class AddRoom
{
    public class Validator : AbstractValidator<Command>
    {
        private readonly IApplicationDbContext _context;
        public Validator(IApplicationDbContext context)
        {
            _context = context;

            RuleLevelCascadeMode = CascadeMode.Stop;
        
            RuleFor(x => x.Capacity)
                .GreaterThanOrEqualTo(1).WithMessage("Room's capacity cannot be less than 1");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(64).WithMessage("Name cannot exceed 64 characters.")
                .Must(BeUnique).WithMessage("Room name already exists.");
        
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(256).WithMessage("Description cannot exceed 256 characters.");
        }

        private bool BeUnique(string name)
        {
            return _context.Rooms.FirstOrDefault(x => x.Name.Equals(name)) is null;
        }
    }
    
    public record Command : IRequest<RoomDto>
    {
        public User CurrentUser { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public int Capacity { get; init; }
        public Guid DepartmentId { get; init; }
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
            var department =
                await _context.Departments.FirstOrDefaultAsync(x => x.Id == request.DepartmentId, cancellationToken);
    
            if (department is null)
            {
                throw new KeyNotFoundException("Department does not exists.");
            }
            
            var room = await _context.Rooms.FirstOrDefaultAsync(r =>
                r.Name.Trim().ToLower().Equals(request.Name.Trim().ToLower()), cancellationToken);
    
            if (room is not null)
            {
                throw new ConflictException("Room name already exists.");
            }
            
            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);
            var entity = new Room
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                NumberOfLockers = 0,
                Capacity = request.Capacity,
                Department = department,
                DepartmentId = request.DepartmentId,
                IsAvailable = true,
                Created = localDateTimeNow,
                CreatedBy = request.CurrentUser.Id,
            };
            
            var log = new RoomLog()
            {
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                ObjectId = entity.Id,
                Time = localDateTimeNow,
                Action = RoomLogMessage.Add,
            };
            var result = await _context.Rooms.AddAsync(entity, cancellationToken);
            await _context.RoomLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<RoomDto>(result.Entity);
        }
    }
}