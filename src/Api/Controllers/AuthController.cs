using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using Api.Controllers.Payload.Requests.Auth;
using Api.Controllers.Payload.Responses;
using Application.Common.Extensions.Logging;
using Application.Common.Interfaces;
using Application.Common.Logging;
using Application.Common.Models;
using Application.Common.Models.Dtos;
using Application.Users.Queries;
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
    private readonly ILogger<AuthController> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuthController(IIdentityService identityService, ILogger<AuthController> logger, IDateTimeProvider dateTimeProvider)
    {
        _identityService = identityService;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
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
    public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
    {
        var result = await _identityService.LoginAsync(loginModel.Email, loginModel.Password);

        return result.Match<IActionResult>(loginSuccess =>
            {
                SetRefreshToken(loginSuccess.AuthResult.RefreshToken);
                SetJweToken(loginSuccess.AuthResult.Token, loginSuccess.AuthResult.RefreshToken);

                var loginResult = new LoginResult()
                {
                    Id = loginSuccess.UserCredentials.Id,
                    Username = loginSuccess.UserCredentials.Username,
                    Email = loginSuccess.UserCredentials.Email,
                    Department = loginSuccess.UserCredentials.Department,
                    Position = loginSuccess.UserCredentials.Position,
                    Role = loginSuccess.UserCredentials.Role,
                    FirstName = loginSuccess.UserCredentials.FirstName,
                    LastName = loginSuccess.UserCredentials.LastName,
                };

                var dateTimeNow = _dateTimeProvider.DateTimeNow;
                using (Logging.PushProperties("Login", loginResult.Id, loginResult.Id))
                {
                    _logger.LogLogin(loginResult.Username, dateTimeNow.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                return Ok(Result<LoginResult>.Succeed(loginResult));
            },
            token =>
            {
                var r = new Result<NotActivatedLoginResult>
                {
                    Data = new NotActivatedLoginResult()
                    {
                        Token = token,
                    }
                };
                return Unauthorized(r);
            });

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
    
    [HttpPost]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrEmpty(request.NewPassword))
        {
            return BadRequest("Password cannot be empty.");
        }
        
        if (!request.NewPassword.Equals(request.ConfirmPassword))
        {
            return BadRequest("Confirm password must match with new password.");
        }

        await _identityService.ResetPassword(request.Token, request.NewPassword);
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