using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos;
using Application.Helpers;
using Application.Identity;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly JweSettings _jweSettings;
    private readonly ApplicationDbContext _context;
    private readonly RSA _encryptionKey;
    private readonly ECDsa _signingKey;
    private readonly IMapper _mapper;

    public IdentityService(TokenValidationParameters tokenValidationParameters, IOptions<JweSettings> jweSettingsOptions, ApplicationDbContext context, RSA encryptionKey, ECDsa signingKey, IMapper mapper)
    {
        _tokenValidationParameters = tokenValidationParameters;
        _jweSettings = jweSettingsOptions.Value;
        _context = context;
        _encryptionKey = encryptionKey;
        _signingKey = signingKey;
        _mapper = mapper;
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(string token, string refreshToken)
    {
        var validatedToken = GetPrincipalFromToken(token);

        if (validatedToken is null)
        {
            throw new AuthenticationException("Invalid token.");
        }

        const string emailClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
        
        var email = validatedToken.Claims.Single(y => y.Type.Equals(emailClaim)).Value;
        
        var user = await _context.Users.FirstOrDefaultAsync(x =>
            x.Username.Equals(email)
            || x.Email!.Equals(email));
        
        if (user is null)
        {
            throw new AuthenticationException("Invalid token.");
        }
        
        var expiryDateUnix =
            long.Parse(validatedToken.Claims.Single(x => x.Type.Equals(JwtRegisteredClaimNames.Exp)).Value);

        var expiryDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddSeconds(expiryDateUnix);

        if (expiryDateTimeUtc > DateTime.UtcNow)
        {
            throw new AuthenticationException("This token has not expired yet.");
        }

        var jti = validatedToken.Claims.Single(x => x.Type.Equals(JwtRegisteredClaimNames.Jti)).Value;
        var storedRefreshToken = await _context.RefreshTokens.SingleOrDefaultAsync(x => x.Token.Equals(Guid.Parse(refreshToken)));

        if (storedRefreshToken is null)
        {
            throw new AuthenticationException("This refresh token does not exist.");
        }

        if (DateTime.UtcNow > storedRefreshToken.ExpiryDateTime.ToDateTimeUnspecified().ToUniversalTime())
        {
            throw new AuthenticationException("This refresh token has expired.");
        }

        if (storedRefreshToken.IsInvalidated)
        {
            throw new AuthenticationException("This refresh token has been invalidated.");
        }

        if (storedRefreshToken.IsUsed)
        {
            throw new AuthenticationException("This refresh token has been used.");
        }

        if (!storedRefreshToken.JwtId.Equals(jti))
        {
            throw new AuthenticationException("This refresh token does not match this Jwt.");
        }

        storedRefreshToken.IsUsed = true;
        _context.RefreshTokens.Update(storedRefreshToken);
        await _context.SaveChangesAsync();

        return await GenerateAuthenticationResultForUserAsync(user);
    }

    private ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var newTokenValidationParameters = new TokenValidationParameters()
            {
                ValidateAudience = _tokenValidationParameters.ValidateAudience,
                ValidateIssuer = _tokenValidationParameters.ValidateIssuer,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = _tokenValidationParameters.ValidateIssuerSigningKey,
                ClockSkew = TimeSpan.Zero,
                // public key for signing
                IssuerSigningKey = _tokenValidationParameters.IssuerSigningKey,

                // private key for encryption
                TokenDecryptionKey = _tokenValidationParameters.TokenDecryptionKey,
            };
            var principal = handler.ValidateToken(token, newTokenValidationParameters, out _);

            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            return null;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.StackTrace);
            return null;
        }
    }

    public async Task<AuthenticationResult> LoginAsync(string email, string password)
    {
        var user = _context.Users.FirstOrDefault(x => x.Email!.Equals(email));

        if (user is null || !user.PasswordHash.Equals(SecurityUtil.Hash(password)))
        {
            throw new AuthenticationException("Username or password is invalid.");
        }
        
        return await GenerateAuthenticationResultForUserAsync(user);
    }

    private async Task<AuthenticationResult> GenerateAuthenticationResultForUserAsync(User user)
    {
        var utcNow = DateTime.UtcNow;
        var authClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, utcNow.ToString(CultureInfo.InvariantCulture)),
        };
        var publicEncryptionKey = new RsaSecurityKey(_encryptionKey.ExportParameters(false)) {KeyId = _jweSettings.EncryptionKeyId};
        var privateSigningKey = new ECDsaSecurityKey(_signingKey) {KeyId = _jweSettings.SigningKeyId};

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(authClaims),
            SigningCredentials = 
                new SigningCredentials(privateSigningKey, SecurityAlgorithms.EcdsaSha256),
            EncryptingCredentials = 
                new EncryptingCredentials(publicEncryptionKey, SecurityAlgorithms.RsaOAEP, SecurityAlgorithms.Aes256CbcHmacSha512),
            Expires = utcNow.Add(_jweSettings.TokenLifetime),
        };

        var handler = new JwtSecurityTokenHandler();

        var token = handler.CreateToken(tokenDescriptor);

        var refreshToken = new RefreshToken()
        {
            JwtId = token.Id,
            User = user,
            CreationDateTime = LocalDateTime.FromDateTime(utcNow),
            ExpiryDateTime = LocalDateTime.FromDateTime(utcNow.AddDays(_jweSettings.RefreshTokenLifetimeInDays))
        };

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
        return new()
        {
            Token = token,
            RefreshToken = _mapper.Map<RefreshTokenDto>(refreshToken)
        };
    }
}