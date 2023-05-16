using Application.Common.Interfaces;
using Application.Helpers;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Users.Commands.CreateUserCommand;

public record CreateUserCommand : IRequest<UserDTO>
{
    public string Username { get; init; }
    public string Email { get; init; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Department Department { get; set; }
    public string Role { get; set; }
    public string Position { get; set; }
    public bool IsActive { get; set; }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDTO>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    public CreateUserCommandHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }
    public async Task<UserDTO> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var entity = new User
        {
            Username = request.Username,
            PasswordHash = SecurityUtil.Hash(request.Password),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Department = request.Department,
            Role = request.Role,
            Position = request.Position,
            IsActive = request.IsActive
        };

        var result = await _uow.UserRepository.CreateUserAsync(entity);
        return _mapper.Map<UserDTO>(result);
    }
}

