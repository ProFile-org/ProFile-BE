using Application.Users.Queries;

namespace Api.Controllers.Payload.Responses;

public class LoginResult
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DepartmentDto Department { get; set; }
    public string Role { get; set; }
    public string Position { get; set; }
}