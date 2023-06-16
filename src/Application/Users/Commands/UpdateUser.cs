using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Messages;
using Application.Identity;
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
        public User CurrentUser { get; init; } = null!;
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
        private readonly IDateTimeProvider _dateTimeProvider;

        public CommandHandler(IApplicationDbContext context, IMapper mapper, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<UserDto> Handle(Command request, CancellationToken cancellationToken)
        {
            // save a roundtrip to db
            if (request.CurrentUser.Role.IsAdmin()
                && UpdateSelf(request.CurrentUser.Id, request.UserId))
            {
                throw new UnauthorizedAccessException("User cannot update this resource.");
            }
            
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id.Equals(request.UserId), cancellationToken: cancellationToken);

            if (user is null)
            {
                throw new KeyNotFoundException("User does not exist.");
            }

            var localDateTimeNow = LocalDateTime.FromDateTime(_dateTimeProvider.DateTimeNow);

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Position = request.Position;
            user.Role = request.Role;
            user.IsActive = request.IsActive;
            user.LastModified = localDateTimeNow;
            user.LastModifiedBy = request.CurrentUser.Id;
            
            var log = new UserLog()
            {
                User = request.CurrentUser,
                UserId = request.CurrentUser.Id,
                Object = user,
                Time = localDateTimeNow,
                Action = UserLogMessages.Update,
            };
            var result = _context.Users.Update(user);
            await _context.UserLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<UserDto>(result.Entity);
        }

        private static bool UpdateSelf(Guid currentUserId, Guid updatingUserId)
            => updatingUserId == currentUserId;
    }
}