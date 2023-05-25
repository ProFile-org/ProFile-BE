using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity.Authentication;

public class JweAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "Cookies";
    public const string TokenCookieName = "JweToken";
    public TokenValidationParameters TokenValidationParameters { get; set; } = new();
}