using Application.Common.Models;
using Application.Identity;
using Application.Staffs.Commands.AddStaff;
using Application.Users.Queries.Physical;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class StaffsController : ApiControllerBase
{
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<StaffDto>>> AddStaff([FromBody] AddStaffCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<StaffDto>.Succeed(result));
    }
}