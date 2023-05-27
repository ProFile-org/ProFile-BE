namespace Api.Controllers.Payload.Requests.Auth;

public class LoginModel
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}