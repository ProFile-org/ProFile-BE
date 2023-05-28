using MediatR;

namespace Application.Users.Queries;

public class GetUserById
{
    public record Query : IRequest<UserDto>
    {
        public Guid UserId { get; init; }
    }
}