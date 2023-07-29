using Application.Common.Models;
using Application.Identity;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class DashboardController : ApiControllerBase
{
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpGet("online-users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> GetOnlineUsers()
    {
        return Ok(Result<int>.Succeed(RequiresRoleAttribute.OnlineUsers.Count));
    }
}