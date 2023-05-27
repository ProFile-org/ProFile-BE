namespace Api.Controllers.Payload.Requests.Auth;

public class RefreshTokenRequest
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}