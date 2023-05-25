using System.IdentityModel.Tokens.Jwt;
using Application.Common.Models.Dtos;
using Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Application.Common.Models;

public class AuthenticationResult
{
    public SecurityToken Token { get; set; }
    public RefreshTokenDto RefreshToken { get; set; }
}