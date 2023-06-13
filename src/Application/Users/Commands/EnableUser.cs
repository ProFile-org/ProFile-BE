using Application.Users.Queries;
using MediatR;

namespace Application.Users.Commands;

public class EnableUser
{
    public record Command : IRequest<UserDto>
    {
        public Guid PerformingUserId { get; init; }
        public Guid UserId { get; init; }
    }
}