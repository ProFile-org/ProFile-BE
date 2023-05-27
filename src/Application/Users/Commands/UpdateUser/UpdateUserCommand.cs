using Application.Users.Queries;
using MediatR;

namespace Application.Users.Commands.UpdateUser;

public record UpdateUserCommand : IRequest<UserDto>
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string Role { get; init; } = null!;
    public string? Position { get; init; }
}