using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Api.Controllers.Payload;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Helpers;
using Infrastructure.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace Api.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly JweSettings _jweSettings;
    private readonly RSA _encryptionKey;
    private readonly ECDsa _signingKey;

    public AuthController(IApplicationDbContext context, IOptions<JweSettings> jweSettings, ECDsa signingKey, RSA encryptionKey)
    {
        _context = context;
        _signingKey = signingKey;
        _encryptionKey = encryptionKey;
        _jweSettings = jweSettings.Value;
    }

    [HttpPost("[action]")]
    public ActionResult<Result<object>> Login([FromBody] LoginModel loginModel)
    {
        var user = _context.Users.FirstOrDefault(x => x.Username.Equals(loginModel.Username)
                                           || x.Email.Equals(loginModel.Username));
        if (user is null)
        {
            return NotFound();
        }

        if (!SecurityUtil.Hash(loginModel.Password).Equals(user.PasswordHash))
        {
            return Unauthorized();
        }
        
        var authClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Iat, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, user.Role),
        };
        var publicEncryptionKey = new RsaSecurityKey(_encryptionKey.ExportParameters(false)) {KeyId = _jweSettings.EncryptionKeyId};
        var privateSigningKey = new ECDsaSecurityKey(_signingKey) {KeyId = _jweSettings.SigningKeyId};

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(authClaims),
            SigningCredentials = 
                new SigningCredentials(privateSigningKey, SecurityAlgorithms.EcdsaSha256),
            EncryptingCredentials = 
                new EncryptingCredentials(publicEncryptionKey, SecurityAlgorithms.RsaOAEP, SecurityAlgorithms.Aes256CbcHmacSha512)
        };

        var handler = new JsonWebTokenHandler();
        
        var token = handler.CreateToken(tokenDescriptor);
        return Ok(new {
                token
            }
        );
    }
}