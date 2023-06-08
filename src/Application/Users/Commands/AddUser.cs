using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Helpers;
using Application.Identity;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Users.Commands;

public class AddUser
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.")
                .MaximumLength(50).WithMessage("Username cannot exceed 64 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Require valid email.")
                .MaximumLength(320).WithMessage("Email length too long.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required.")
                .MaximumLength(64).WithMessage("Role cannot exceed 64 characters.")
                .Must(BeNotAdmin).WithMessage("Cannot add a user as Administrator.");

            RuleFor(x => x.FirstName)
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.Position)
                .MaximumLength(64).WithMessage("Position cannot exceed 64 characters.");
        }

        private static bool BeNotAdmin(string role)
        {
            return !role.Equals(IdentityData.Roles.Admin);
        }
    }

    public record Command : IRequest<UserDto>
    {
        public string Username { get; init; } = null!;
        public string Email { get; init; } = null!;
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public Guid DepartmentId { get; init; }
        public string Role { get; init; } = null!;
        public string? Position { get; init; }
    }

    public class AddUserCommandHandler : IRequestHandler<Command, UserDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ISecurityService _securityService;

        public AddUserCommandHandler(IApplicationDbContext context, IMapper mapper, ISecurityService securityService)
        {
            _context = context;
            _mapper = mapper;
            _securityService = securityService;
        }

        public async Task<UserDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(
                x => x.Username.Equals(request.Username) || x.Email.Equals(request.Email), cancellationToken);

            if (user is not null)
            {
                throw new ConflictException("Username or Email has been taken.");
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(x => x.Id == request.DepartmentId, cancellationToken);

            if (department is null)
            {
                throw new KeyNotFoundException("Department does not exist.");
            }

            var password = StringUtil.RandomPassword();
            var salt = StringUtil.RandomSalt();
            
            var entity = new User
            {
                Username = request.Username,
                PasswordHash = _securityService.Hash(password, salt),
                PasswordSalt = salt,
                Email = request.Email,
                FirstName = request.FirstName?.Trim(),
                LastName = request.LastName?.Trim(),
                Department = department,
                Role = request.Role,
                Position = request.Position,
                IsActive = true,
                IsActivated = false,
                Created = LocalDateTime.FromDateTime(DateTime.UtcNow)
            };
            entity.AddDomainEvent(new UserCreatedEvent(entity.Email, password));
            var result = await _context.Users.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<UserDto>(result.Entity);
        }
    }
}