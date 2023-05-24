namespace Api.Controllers.Payload.Responses;

public class ApiAuthenticationResult
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}