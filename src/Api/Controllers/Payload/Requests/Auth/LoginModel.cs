namespace Api.Controllers.Payload.Requests.Auth;

/// <summary>
/// Login credentials to login
/// </summary>
public class LoginModel
{
    /// <summary>
    /// Email of user
    /// </summary>
    public string Email { get; set; } = null!;
    /// <summary>
    /// Password of user
    /// </summary>
    public string Password { get; set; } = null!;
}