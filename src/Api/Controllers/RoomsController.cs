using Api.Controllers.Payload.Requests.Lockers;
using Api.Controllers.Payload.Requests.Rooms;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Application.Rooms.Commands;
using Application.Rooms.Queries;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class RoomsController : ApiControllerBase
{
    /// <summary>
    /// Get a room by id
    /// </summary>
    /// <param name="roomId">Id of the room to be retrieved</param>
    /// <returns>A RoomDto of the retrieved room</returns>
    [HttpGet("{roomId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<RoomDto>>> GetById([FromRoute] Guid roomId)
    {
        var query = new GetRoomById.Query()
        {
            RoomId = roomId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<RoomDto>.Succeed(result));
    }
    
    /// <summary>
    /// Get all rooms paginated
    /// </summary>
    /// <param name="queryParameters">Get all rooms paginated details</param>
    /// <returns>A paginated list of rooms</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<PaginatedList<RoomDto>>>> GetAllPaginated(
        [FromQuery] GetAllRoomsPaginatedQueryParameters queryParameters)
    {
        var query = new GetAllRoomsPaginated.Query()
        {
            SearchTerm = queryParameters.SearchTerm,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<RoomDto>>.Succeed(result));
    }
    
    /// <summary>
    /// Get empty containers in a room
    /// </summary>
    /// <param name="queryParameters">Get empty containers paginated details</param>
    /// <returns>A paginated list of EmptyLockerDto</returns>
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPost("empty-containers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedList<EmptyLockerDto>>> GetEmptyContainers(
        [FromRoute] Guid roomId,
        [FromQuery] GetEmptyContainersPaginatedQueryParameters queryParameters)
    {
        var query = new GetEmptyContainersPaginated.Query()
        {
            RoomId = roomId,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<EmptyLockerDto>>.Succeed(result));
    }
    
    /// <summary>
    /// Add a room
    /// </summary>
    /// <param name="request">Add room details</param>
    /// <returns>A RoomDto of the added room</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<RoomDto>>> AddRoom([FromBody] AddRoomRequest request)
    {
        var command = new AddRoom.Command()
        {
            Name = request.Name,
            Description = request.Description,
            Capacity = request.Capacity,
            DepartmentId = request.DepartmentId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<RoomDto>.Succeed(result));
    }
    
    /// <summary>
    /// Remove a room
    /// </summary>
    /// <param name="roomId">Id of the room to be removed</param>
    /// <returns>A RoomDto of the removed room</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpDelete("{roomId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<RoomDto>>> RemoveRoom([FromRoute] Guid roomId)
    {
        var command = new RemoveRoom.Command()
        {
            RoomId = roomId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<RoomDto>.Succeed(result));
    }

    /// <summary>
    /// Enable a room
    /// </summary>
    /// <param name="roomId">Id of the room to be enabled</param>
    /// <returns>A RoomDto of the enabled room</returns>
    [HttpPut("enable/{roomId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<RoomDto>>> EnableRoom([FromRoute] Guid roomId)
    {
        var command = new EnableRoom.Command()
        {
            RoomId = roomId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<RoomDto>.Succeed(result));
    }
    
    /// <summary>
    /// Disable a room
    /// </summary>
    /// <param name="roomId">Id of the room to be disabled</param>
    /// <returns>A RoomDto of the disabled room</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPut("disable/{roomId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<RoomDto>>> DisableRoom([FromRoute] Guid roomId)
    {
        var command = new DisableRoom.Command()
        {
            RoomId = roomId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<RoomDto>.Succeed(result));
    }

    /// <summary>
    /// Update a room
    /// </summary>
    /// <param name="roomId">Id of the room to be updated</param>
    /// <param name="request">Update room details</param>
    /// <returns>A RoomDto of the updated room</returns>
    [HttpPut("{roomId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<RoomDto>>> Update([FromRoute] Guid roomId, [FromBody] UpdateRoomRequest request)
    {
        var command = new UpdateRoom.Command()
        {
            RoomId = roomId,
            Name = request.Name,
            Description = request.Description,
            Capacity = request.Capacity,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<RoomDto>.Succeed(result));
    }
}