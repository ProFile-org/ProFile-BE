namespace Api.Controllers.Payload.Requests.Users;

public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Role { get; set; } = null!;
    public string? Position { get; set; }
}