using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos;
using Application.Helpers;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities;
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
    private readonly IApplicationDbContext _applicationDbContext;
    private readonly IAuthDbContext _authDbContext;
    private readonly RSA _encryptionKey;
    private readonly ECDsa _signingKey;
    private readonly IMapper _mapper;
    private readonly SecuritySettings _securitySettings;

    public IdentityService(
        TokenValidationParameters tokenValidationParameters, 
        IOptions<JweSettings> jweSettingsOptions, 
        IApplicationDbContext applicationDbContext, 
        IAuthDbContext authDbContext, 
        RSA encryptionKey, 
        ECDsa signingKey, 
        IMapper mapper, 
        IOptions<SecuritySettings> securitySettingsOptions)
    {
        _tokenValidationParameters = tokenValidationParameters;
        _jweSettings = jweSettingsOptions.Value;
        _applicationDbContext = applicationDbContext;
        _authDbContext = authDbContext; 
        _encryptionKey = encryptionKey;
        _signingKey = signingKey;
        _mapper = mapper;
        _securitySettings = securitySettingsOptions.Value;
    }

    public async Task<bool> Validate(string token, string refreshToken)
    {
        var validatedToken = GetPrincipalFromToken(token);

        if (validatedToken is null)
        {
            return false;
        }

        const string emailClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
        
        var email = validatedToken.Claims.Single(y => y.Type.Equals(emailClaim)).Value;
        
        var user = await _applicationDbContext.Users.FirstOrDefaultAsync(x =>
            x.Username.Equals(email)
            || x.Email!.Equals(email));
        
        if (user is null)
        {
            return false;
        }
        
        var expiryDateUnix =
            long.Parse(validatedToken.Claims.Single(x => x.Type.Equals(JwtRegisteredClaimNames.Exp)).Value);

        var expiryDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddSeconds(expiryDateUnix);

        if (expiryDateTimeUtc < DateTime.UtcNow)
        {
            return false;
        }

        var jti = validatedToken.Claims.Single(x => x.Type.Equals(JwtRegisteredClaimNames.Jti)).Value;
        var storedRefreshToken = await _authDbContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token.Equals(Guid.Parse(refreshToken)));

        if (storedRefreshToken is null)
        {
            return false;
        }

        if (DateTime.UtcNow > storedRefreshToken.ExpiryDateTime.ToDateTimeUnspecified().ToUniversalTime())
        {
            return false;
        }

        if (storedRefreshToken.IsInvalidated)
        {
            return false;
        }

        if (storedRefreshToken.IsUsed)
        {
            return false;
        }

        return storedRefreshToken.JwtId.Equals(jti);
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
        
        var user = await _applicationDbContext.Users.FirstOrDefaultAsync(x =>
            x.Username.Equals(email)
            || x.Email!.Equals(email));
        
        if (user is null)
        {
            throw new AuthenticationException("Invalid token.");
        }

        var jti = validatedToken.Claims.Single(x => x.Type.Equals(JwtRegisteredClaimNames.Jti)).Value;
        var storedRefreshToken = await _authDbContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token.Equals(Guid.Parse(refreshToken)));

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

        var result = await GenerateAuthenticationResultForUserAsync(user);

        return result;
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
        catch
        {
            return null;
        }
    }

    public async Task<(AuthenticationResult, UserDto)> LoginAsync(string email, string password)
    {
        var user = _applicationDbContext.Users
            .Include(x => x.Department)
            .FirstOrDefault(x => x.Email!.Equals(email));

        if (user is null || !user.PasswordHash.Equals(password.HashPasswordWith(user.PasswordSalt, _securitySettings.Pepper)))
        {
            throw new AuthenticationException("Username or password is invalid.");
        }
        
        var existedRefreshTokens = _authDbContext.RefreshTokens.Where(x => x.User.Email!.Equals(user.Email));

        _authDbContext.RefreshTokens.RemoveRange(existedRefreshTokens);
        await _authDbContext.SaveChangesAsync(CancellationToken.None);
        
        return (await GenerateAuthenticationResultForUserAsync(user), _mapper.Map<UserDto>(user));
    }

    public async Task LogoutAsync(string token, string refreshToken)
    {
        var validatedToken = GetPrincipalFromToken(token);

        if (validatedToken is null)
        {
            throw new AuthenticationException("Invalid token.");
        }

        var jti = validatedToken.Claims.Single(x => x.Type.Equals(JwtRegisteredClaimNames.Jti)).Value;
        var storedRefreshToken =
            await _authDbContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token.Equals(Guid.Parse(refreshToken)));

        if (storedRefreshToken is null) return;
        
        if (!storedRefreshToken!.JwtId.Equals(jti))
        {
            throw new AuthenticationException("This refresh token does not match this Jwt.");
        }

        _authDbContext.RefreshTokens.Remove(storedRefreshToken);
        await _authDbContext.SaveChangesAsync(CancellationToken.None);
    }

    public async Task ResetPassword(string token, string newPassword)
    {
        var tokenHash = SecurityUtil.Hash(token);
        var resetPasswordToken = await _authDbContext.ResetPasswordTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash.Equals(tokenHash));
        if (resetPasswordToken is null)
        {
            throw new KeyNotFoundException("Token is invalid.");
        }

        if (resetPasswordToken.IsInvalidated)
        {
            throw new ConflictException("Token is invalid.");
        }

        var user = resetPasswordToken.User;

        if (user.IsActivated is false)
        {
            user.IsActivated = true;
        }
        var salt = StringUtil.RandomSalt();
        user.PasswordSalt = salt;
        user.PasswordHash = newPassword.HashPasswordWith(salt, newPassword);
        resetPasswordToken.IsInvalidated = true;
        await _applicationDbContext.SaveChangesAsync(CancellationToken.None);
        await _authDbContext.SaveChangesAsync(CancellationToken.None);
    }

    private async Task<AuthenticationResult> GenerateAuthenticationResultForUserAsync(User user)
    {
        var token = CreateJweToken(user);
        var utcNow = DateTime.UtcNow;

        var refreshToken = new RefreshToken()
        {
            JwtId = token.Id,
            User = user,
            CreationDateTime = LocalDateTime.FromDateTime(utcNow),
            ExpiryDateTime = LocalDateTime.FromDateTime(utcNow.AddDays(_jweSettings.RefreshTokenLifetimeInDays))
        };

        await _authDbContext.RefreshTokens.AddAsync(refreshToken);
        await _authDbContext.SaveChangesAsync(CancellationToken.None);
        return new()
        {
            Token = token,
            RefreshToken = _mapper.Map<RefreshTokenDto>(refreshToken)
        };
    }

    private SecurityToken CreateJweToken(User user)
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

        return token;
    }
}