using Application.Users.Queries;
using MediatR;

namespace Application.Users.Commands.EnableUser;

public record EnableUserCommand : IRequest<UserDto>
{
    public Guid UserId { get; init; }
}