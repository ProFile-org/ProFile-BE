using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthenticationResult> RefreshTokenAsync(string token, string refreshToken);
    Task<AuthenticationResult> LoginAsync(string email, string password);
}