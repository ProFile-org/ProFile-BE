using Microsoft.AspNetCore.Authentication;

namespace Infrastructure.Authentication;

public class JweAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "Bearer";
    public string TokenHeaderName { get; set; } = "Authorization";
}