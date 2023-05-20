using Application.Common.Models;
using Application.Staffs.Commands.CreateStaff;
using Application.Users.Queries.Physical;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class StaffsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Result<StaffDto>>> CreateStaff([FromBody] CreateStaffCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<StaffDto>.Succeed(result));
    }
}