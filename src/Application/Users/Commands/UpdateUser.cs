using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Logging;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Users.Commands;

public class UpdateUser
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.FirstName)
                .MaximumLength(50).WithMessage("FirstName can not exceed 50 characters.");

            RuleFor(x => x.LastName)
                .MaximumLength(50).WithMessage("LastName can not exceed 50 characters.");

            RuleFor(x => x.Position)
                .MaximumLength(64).WithMessage("Position can not exceed 64 characters.");
        }
    }
    public record Command : IRequest<UserDto>
    {
        public User PerformingUser { get; init; } = null!;
        public Guid UserId { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? Position { get; init; }
        public string Role { get; init; } = null!;
        public bool IsActive { get; init; } }
    
    public class CommandHandler : IRequestHandler<Command, UserDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<UserDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id.Equals(request.UserId), cancellationToken: cancellationToken);

            if (user is null)
            {
                throw new KeyNotFoundException("User does not exist.");
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Position = request.Position;
            user.Role = request.Role;
            user.IsActive = request.IsActive;
            user.LastModified = LocalDateTime.FromDateTime(DateTime.Now);
            user.LastModifiedBy = request.PerformingUser.Id;
            var log = new UserLog()
            {
                User = request.PerformingUser,
                UserId = request.PerformingUser.Id,
                Object = user,
                Time = LocalDateTime.FromDateTime(DateTime.Now),
                Action = UserLogMessages.Update,
            };
            var result = _context.Users.Update(user);
            await _context.UserLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<UserDto>(result.Entity);
        }
    }
}