using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using Infrastructure.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity.Authentication;

public class JweAuthenticationHandler : AuthenticationHandler<JweAuthenticationOptions>
{
    private readonly JweSettings _jweSettings;
    
    private readonly RSA _encryptionKey;
    private readonly ECDsa _signingKey;
    
    public JweAuthenticationHandler(
        IOptionsMonitor<JweAuthenticationOptions> options, 
        ILoggerFactory logger, UrlEncoder encoder, 
        ISystemClock clock, IOptions<JweSettings> jweSettings, 
        RSA encryptionKey, ECDsa signingKey) : base(options, logger, encoder, clock)
    {
        _encryptionKey = encryptionKey;
        _signingKey = signingKey;
        _jweSettings = jweSettings.Value;
    }
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        //check header first
        if (!Request.Headers.ContainsKey(Options.TokenHeaderName))
        {
            return AuthenticateResult.Fail($"Missing header: {Options.TokenHeaderName}");
        }

        //get the header and validate
        string token = Request.Headers[Options.TokenHeaderName]!;
        token = token.Substring(token.IndexOf(" ", StringComparison.Ordinal) + 1);

        var privateEncryptionKey = new RsaSecurityKey(_encryptionKey) {KeyId = _jweSettings.EncryptionKeyId};
        var publicSigningKey = new ECDsaSecurityKey(ECDsa.Create(_signingKey.ExportParameters(false))) {KeyId = _jweSettings.SigningKeyId};

        var handler = new JsonWebTokenHandler();
        var result = handler.ValidateToken(token,
            new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = true,
                // public key for signing
                IssuerSigningKey = publicSigningKey,
        
                // private key for encryption
                TokenDecryptionKey = privateEncryptionKey
            });

        if (!result.IsValid)
        {
            return AuthenticateResult.Fail("Invalid token.");
        }

        var claimsPrincipal = new ClaimsPrincipal(result.ClaimsIdentity);

        return AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, this.Scheme.Name));
    }
}