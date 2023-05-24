using System.IdentityModel.Tokens.Jwt;
using Application.Common.Interfaces;
using Application.Identity;

namespace Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetRole()
    {
        var roleClaim = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type.Equals(IdentityData.Claims.Role));
        if (roleClaim is null)
        {
            throw new UnauthorizedAccessException();
        }
        return roleClaim.Value;
    }
}