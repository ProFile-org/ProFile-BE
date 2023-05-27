using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using Api.Controllers.Payload.Requests;
using Api.Controllers.Payload.Requests.Auth;
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

    /// <summary>
    /// Login
    /// </summary>
    /// <param name="loginModel">Login credentials</param>
    /// <returns>A LoginResult indicating the result of logging in</returns>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<LoginResult>>> Login([FromBody] LoginModel loginModel)
    {
        var result = await _identityService.LoginAsync(loginModel.Email, loginModel.Password);
        
        SetRefreshToken(result.AuthResult.RefreshToken);
        SetJweToken(result.AuthResult.Token, result.AuthResult.RefreshToken);

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
    
    /// <summary>
    /// Refresh session and token
    /// </summary>
    /// <returns>An IActionResult indicating the result of refreshing token</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[nameof(RefreshToken)];
        var jweToken = Request.Cookies["JweToken"];

        try
        {
            var authResult = await _identityService.RefreshTokenAsync(jweToken!, refreshToken!);
            
            SetRefreshToken(authResult.RefreshToken);
            SetJweToken(authResult.Token, authResult.RefreshToken);
        }
        catch (AuthenticationException)
        {
            RemoveJweToken();
            RemoveRefreshToken();
            throw;
        }

        return Ok();
    }
    
    /// <summary>
    /// Validate current user
    /// </summary>
    /// <returns>An IActionResult indicating the result of validating the user</returns>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Validate()
    {
        return Ok();
    }

    /// <summary>
    /// Logout of the system
    /// </summary>
    /// <returns>An IActionResult indicating the result of logging out of the system</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[nameof(RefreshToken)];
        var jweToken = Request.Cookies["JweToken"];
        
        RemoveJweToken();
        RemoveRefreshToken();
        
        await _identityService.LogoutAsync(jweToken!, refreshToken!);

        return Ok();
    }

    private void SetJweToken(SecurityToken jweToken, RefreshTokenDto newRefreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = newRefreshToken.ExpiryDateTime
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
    
    private void RemoveJweToken()
    {
        Response.Cookies.Delete("JweToken");
    }
    
    private void RemoveRefreshToken()
    {
        Response.Cookies.Delete(nameof(RefreshToken));
    }
}