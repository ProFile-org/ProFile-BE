using Api.Controllers.Payload.Requests.Staffs;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Application.Rooms.Queries;
using Application.Staffs.Commands;
using Application.Staffs.Queries;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class StaffsController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public StaffsController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get a staff by id
    /// </summary>
    /// <param name="staffId">Id of the staff to be retrieved</param>
    /// <returns>A StaffDto of the retrieved staff</returns>
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpGet("{staffId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<StaffDto>>> GetById(
        [FromRoute] Guid staffId)
    {
        var query = new GetStaffById.Query()
        {
            StaffId = staffId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<StaffDto>.Succeed(result));
    }
    
    /// <summary>
    /// Get room by staff id
    /// </summary>
    /// <param name="staffId">Id of the staff to retrieve room</param>
    /// <returns>A RoomDto of the retrieved room</returns>
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpGet("{staffId:guid}/rooms")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<RoomDto>>> GetRoomByStaffId(
        [FromRoute] Guid staffId)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var query = new GetRoomByStaffId.Query()
        {
            CurrentUser = currentUser,
            StaffId = staffId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<RoomDto>.Succeed(result));
    }
    
    /// <summary>
    /// Get all staffs paginated
    /// </summary>
    /// <param name="queryParameters">Get all staffs query parameters</param>
    /// <returns>A paginated list of StaffDto</returns>
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
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
    /// Assign a staff
    /// </summary>
    /// <param name="request">Add staff details</param>
    /// <returns>A StaffDto of the added staff</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<StaffDto>>> Assign(
        [FromBody] AddStaffRequest request)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var command = new AssignStaff.Command()
        {
            CurrentUser = currentUser,
            RoomId = request.RoomId,
            StaffId = request.StaffId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<StaffDto>.Succeed(result));
    }

    /// <summary>
    /// Remove a staff from room
    /// </summary>
    /// <param name="staffId">Id of the staff to be removed from room</param>
    /// <returns>A StaffDto of the removed staff</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPut("{staffId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<StaffDto>>> RemoveStaffFromRoom(
        [FromRoute] Guid staffId)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var command = new RemoveStaffFromRoom.Command()
        {
            CurrentUser = currentUser,
            StaffId = staffId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<StaffDto>.Succeed(result));
    }
}