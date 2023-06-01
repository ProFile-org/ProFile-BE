using Api.Controllers.Payload.Requests.Staffs;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Application.Staffs.Commands;
using Application.Staffs.Queries;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class StaffsController : ApiControllerBase
{
    /// <summary>
    /// Get a staff by id
    /// </summary>
    /// <param name="staffId">Id of the staff to be retrieved</param>
    /// <returns>A StaffDto of the retrieved staff</returns>
    [HttpGet("{staffId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<StaffDto>>> GetById([FromRoute] Guid staffId)
    {
        var query = new GetStaffById.Query()
        {
            StaffId = staffId
        };
        var result = await Mediator.Send(query);
        return Ok(Result<StaffDto>.Succeed(result));
    }
    
    /// <summary>
    /// Get a staff by room
    /// </summary>
    /// <param name="roomId">Id of the room to retrieve staff</param>
    /// <returns>A StaffDto of the retrieved staff</returns>
    [HttpGet("get-by-room/{roomId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<StaffDto>>> GetByRoom([FromRoute] Guid roomId)
    {
        var query = new GetStaffByRoom.Query()
        {
            RoomId = roomId
        };
        var result = await Mediator.Send(query);
        return Ok(Result<StaffDto>.Succeed(result));
    }
    
    /// <summary>
    /// Get all staffs paginated
    /// </summary>
    /// <param name="queryParameters">Get all staffs query parameters</param>
    /// <returns>A paginated list of StaffDto</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<PaginatedList<StaffDto>>>> GetAllPaginated(
        [FromQuery] GetAllStaffsPaginatedQueryParameters queryParameters)
    {
        var query = new GetAllStaffsPaginated.Query()
        {
            SearchTerm = queryParameters.SearchTerm,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<StaffDto>>.Succeed(result));
    }
    
    /// <summary>
    /// Add a staff
    /// </summary>
    /// <param name="request">Add staff details</param>
    /// <returns>A StaffDto of the added staff</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<StaffDto>>> Add([FromBody] AddStaffRequest request)
    {
        var command = new AddStaff.Command()
        {
            RoomId = request.RoomId,
            UserId = request.UserId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<StaffDto>.Succeed(result));
    }

    /// <summary>
    /// Remove a staff from room
    /// </summary>
    /// <param name="staffId">Id of the staff to be removed from room</param>
    /// <returns>A StaffDto of the removed staff</returns>
    [HttpPut("{staffId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<StaffDto>>> RemoveFromRoom(
        [FromRoute] Guid staffId)
    {
        var command = new RemoveStaffFromRoom.Command()
        {
            StaffId = staffId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<StaffDto>.Succeed(result));
    }

    /// <summary>
    /// Remove a staff
    /// </summary>
    /// <param name="staffId">Id of the staff to be removed</param>
    /// <returns>A StaffDto of the removed staff</returns>
    [HttpDelete("{staffId:guid}")]
    public async Task<ActionResult<Result<StaffDto>>> Remove(
        [FromRoute] Guid staffId)
    {
        var command = new RemoveStaff.Command()
        {
            StaffId = staffId
        };

        var result = await Mediator.Send(command);
        return Ok(Result<StaffDto>.Succeed(result));
    }
}