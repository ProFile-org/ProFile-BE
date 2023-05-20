using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Helpers;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Application.Users.Commands.CreateUser;

public record CreateUserCommand : IRequest<UserDto>
{
    public string Username { get; init; }
    public string Email { get; init; }
    public string Password { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public Guid DepartmentId { get; init; }
    public string Role { get; init; }
    public string Position { get; init; }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    public CreateUserCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(
            x => x.Username.Equals(request.Username) || x.Email.Equals(request.Email), cancellationToken);

        if (user is not null)
        {
            throw new ConflictException("Username or Email has been taken");
        }

        var department = await _context.Departments
                .FirstOrDefaultAsync(x => x.Id == request.DepartmentId, cancellationToken);
        
        if (department is null)
        {
            throw new KeyNotFoundException("Department does not exist");
        }
        
        var entity = new User
        {
            Username = request.Username,
            PasswordHash = SecurityUtil.Hash(request.Password),
            Email = request.Email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Department = department,
            Role = request.Role,
            Position = request.Position,
            IsActive = true,
            IsActivated = false,
            Created = LocalDateTime.FromDateTime(DateTime.UtcNow)
        };
        entity.AddDomainEvent(new UserCreatedEvent(entity));
        var result = await _context.Users.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<UserDto>(result.Entity);
    }
}

