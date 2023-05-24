using System.IdentityModel.Tokens.Jwt;
using Api.Controllers.Payload.Requests;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos;
using Domain.Entities;
using Application.Helpers;
using Application.Identity;
using Infrastructure.Shared;
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
    public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
    {
        var authResult = await _identityService.LoginAsync(loginModel.Email, loginModel.Password);
        
        SetRefreshToken(authResult.RefreshToken);
        SetJweToken(authResult.Token);
        
        return Ok();
    }

    [Authorize]
    [HttpPost]
    public ActionResult<Result<Result<AuthenticationResult>>> Logout()
    {
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
}