using Application.Common.Models;
using Application.Users.Queries;

namespace Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthenticationResult> RefreshTokenAsync(string token, string refreshToken);
    Task<(AuthenticationResult AuthResult, UserDto UserCredentials)> LoginAsync(string email, string password);
    Task<bool> LogoutAsync(string token, string refreshToken);
}