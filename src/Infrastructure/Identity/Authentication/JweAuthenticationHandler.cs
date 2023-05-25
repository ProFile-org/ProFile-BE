using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using Infrastructure.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity.Authentication;

public class JweAuthenticationHandler : AuthenticationHandler<JweAuthenticationOptions>
{
    
    public JweAuthenticationHandler(
        IOptionsMonitor<JweAuthenticationOptions> options, 
        ILoggerFactory logger, UrlEncoder encoder, 
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        //check header first
        if (!Request.Cookies.ContainsKey(JweAuthenticationOptions.TokenCookieName))
        {
            return AuthenticateResult.Fail($"Missing cookie: {JweAuthenticationOptions.TokenCookieName}");
        }

        //get the header and validate
        var token = Request.Cookies[JweAuthenticationOptions.TokenCookieName]!;

        var handler = new JwtSecurityTokenHandler();

        try
        {
            var claimsPrincipal = handler.ValidateToken(token,
                Options.TokenValidationParameters, out var validatedToken);

            Context.User = claimsPrincipal;
            
            return validatedToken is null
                ? AuthenticateResult.Fail("Invalid token.")
                : AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name));
        }
        catch (SecurityTokenExpiredException ex)
        {
            return AuthenticateResult.Fail(ex);
        }
        catch (SecurityTokenKeyWrapException ex)
        {
            return AuthenticateResult.Fail(ex);
        }
        catch (Exception ex)
        {
            return AuthenticateResult.Fail(ex);
        }
    }
}