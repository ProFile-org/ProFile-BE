using Application.Users.Queries;
using MediatR;

namespace Application.Users.Commands;

public class UpdateUser
{
    public record Command : IRequest<UserDto>
    {
        public Guid UserId { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string Role { get; init; } = null!;
        public string? Position { get; init; }
    }
}