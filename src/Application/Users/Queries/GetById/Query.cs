using MediatR;

namespace Application.Users.Queries.GetById;

public record Query : IRequest<UserDto>
{
    public Guid UserId { get; init; }
}