using MediatR;

namespace Application.Users.Queries.GetUserById;

public record GetUserByIdQuery : IRequest<UserDto>
{
    public Guid UserId { get; init; }
}