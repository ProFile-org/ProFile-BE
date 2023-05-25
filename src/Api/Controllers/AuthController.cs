using System.IdentityModel.Tokens.Jwt;
using Api.Controllers.Payload.Requests;
using Api.Controllers.Payload.Responses;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]/[action]")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public AuthController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<LoginResult>>> Login([FromBody] LoginModel loginModel)
    {
        var result = await _identityService.LoginAsync(loginModel.Email, loginModel.Password);
        
        SetRefreshToken(result.AuthResult.RefreshToken);
        SetJweToken(result.AuthResult.Token);

        var loginResult = new LoginResult()
        {
            Id = result.UserCredentials.Id,
            Username = result.UserCredentials.Username,
            Email = result.UserCredentials.Email,
            Department = result.UserCredentials.Department,
            Position = result.UserCredentials.Position,
            Role = result.UserCredentials.Role,
            FirstName = result.UserCredentials.FirstName,
            LastName = result.UserCredentials.LastName,
        };
        
        return Ok(Result<LoginResult>.Succeed(loginResult));
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[nameof(RefreshToken)];
        var jweToken = Request.Cookies["JweToken"];

        var loggedOut = await _identityService.LogoutAsync(jweToken!, refreshToken!);

        if (!loggedOut) return Ok();
        
        RemoveJweToken(jweToken);
        RemoveRefreshToken(refreshToken);

        return Ok();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[nameof(RefreshToken)];
        var jweToken = Request.Cookies["JweToken"];
        
        var authResult = await _identityService.RefreshTokenAsync(jweToken!, refreshToken!);
        
        SetRefreshToken(authResult.RefreshToken);
        SetJweToken(authResult.Token);
        
        return Ok();
    }
    
    private void SetJweToken(SecurityToken jweToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
        };
        var handler = new JwtSecurityTokenHandler();
        Response.Cookies.Append("JweToken", handler.WriteToken(jweToken), cookieOptions);
    }
    
    private void SetRefreshToken(RefreshTokenDto newRefreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = newRefreshToken.ExpiryDateTime
        };
        Response.Cookies.Append(nameof(RefreshToken), newRefreshToken.Token.ToString(), cookieOptions);
    }
    
    private void RemoveJweToken(string jweToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTimeOffset.FromUnixTimeSeconds(0)
        };
        Response.Cookies.Append("JweToken", jweToken, cookieOptions);
    }
    
    private void RemoveRefreshToken(string newRefreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTimeOffset.FromUnixTimeSeconds(0)
        };
        Response.Cookies.Append(nameof(RefreshToken), newRefreshToken, cookieOptions);
    }
}