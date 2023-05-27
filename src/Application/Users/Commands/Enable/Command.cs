using Application.Users.Queries;
using MediatR;

namespace Application.Users.Commands.Enable;

public record Command : IRequest<UserDto>
{
    public Guid UserId { get; init; }
}