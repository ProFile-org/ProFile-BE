namespace Api.Controllers.Payload.Requests.Users;

public class AddUserRequest
{
    public string Username { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public Guid DepartmentId { get; init; }
    public string Role { get; init; } = null!;
    public string? Position { get; init; }
}