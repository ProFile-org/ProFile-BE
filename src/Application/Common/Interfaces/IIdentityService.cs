using Application.Common.Models;
using Application.Users.Queries;
using Domain.Entities;
using OneOf;

namespace Application.Common.Interfaces;

public interface IIdentityService
{
    Task<bool> Validate(string token, string refreshToken);
    Task<AuthenticationResult> RefreshTokenAsync(string token, string refreshToken);
    Task<OneOf<(AuthenticationResult AuthResult, UserDto UserCredentials), string>> LoginAsync(string email, string password);
    Task LogoutAsync(string token, string refreshToken);
    Task ResetPassword(string token, string newPassword);
}