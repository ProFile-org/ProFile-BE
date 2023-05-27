using Api.Controllers.Payload.Requests.Staffs;
using Application.Common.Models;
using Application.Identity;
using Application.Staffs.Commands.CreateStaff;
using Application.Staffs.Commands.RemoveStaffFromRoom;
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
    public async Task<ActionResult<Result<StaffDto>>> CreateStaff([FromBody] CreateStaffCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<StaffDto>.Succeed(result));
    }
    
    /// <summary>
    /// Remove a staff from room
    /// </summary>
    /// <param name="staffId">Id of the staff to be removed from room</param>
    /// <param name="request">Remove details</param>
    /// <returns>A StaffDto of the removed staff</returns>
    [HttpPut("{staffId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<StaffDto>>> RemoveStaffFromRoom([FromRoute] Guid staffId, [FromBody] RemoveStaffFromRoomRequest request)
    {
        var command = new RemoveStaffFromRoomCommand()
        {
            StaffId = staffId,
            RoomId = request.RoomId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<StaffDto>.Succeed(result));
    }
}