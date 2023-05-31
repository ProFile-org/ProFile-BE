namespace Api.Controllers.Payload.Requests.Auth;

/// <summary>
/// Request details to refresh token
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Access token
    /// </summary>
    public string Token { get; set; } = null!;
    /// <summary>
    /// Refresh token
    /// </summary>
    public string RefreshToken { get; set; } = null!;
}