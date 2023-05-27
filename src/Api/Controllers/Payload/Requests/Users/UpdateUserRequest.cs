namespace Api.Controllers.Payload.Requests.Users;

public class UpdateUserRequest
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Role { get; set; } = null!;
    public string? Position { get; set; }
}