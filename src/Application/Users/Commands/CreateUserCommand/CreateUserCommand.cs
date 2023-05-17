using Application.Common.Interfaces;
using Application.Helpers;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Users.Commands.CreateUserCommand;

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
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    public CreateUserCommandHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }
    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var department = await _uow.DepartmentRepository.GetByIdAsync(request.DepartmentId);
        if (department is null)
        {
            throw new KeyNotFoundException($"department {request.DepartmentId} does not exist");
        }
        var entity = new User
        {
            Username = request.Username,
            PasswordHash = SecurityUtil.Hash(request.Password),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Department = department,
            Role = request.Role,
            Position = request.Position,
            IsActive = true,
            IsActivated = false
        };

        var result = await _uow.UserRepository.CreateUserAsync(entity);
        await _uow.Commit();
        return _mapper.Map<UserDto>(result);
    }
}

