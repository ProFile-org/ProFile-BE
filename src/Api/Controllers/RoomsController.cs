using Api.Controllers.Payload.Requests.Lockers;
using Api.Controllers.Payload.Requests.Rooms;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Application.Rooms.Commands.AddRoom;
using Application.Rooms.Commands.DisableRoom;
using Application.Rooms.Commands.EnableRoom;
using Application.Rooms.Commands.RemoveRoom;
using Application.Rooms.Commands.UpdateRoom;
using Application.Rooms.Queries.GetAllRoomPaginated;
using Application.Rooms.Queries.GetEmptyContainersPaginated;
using Application.Rooms.Queries.GetRoomById;
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
        var query = new GetRoomByIdQuery()
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
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<PaginatedList<RoomDto>>>> GetAllPaginated(
        [FromQuery] GetAllLockersPaginatedQueryParameters queryParameters)
    {
        var query = new GetAllRoomsPaginatedQuery()
        {
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
        var query = new GetEmptyContainersPaginatedQuery()
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
    /// <param name="command">Add room details</param>
    /// <returns>A RoomDto of the added room</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<RoomDto>>> AddRoom([FromBody] AddRoomCommand command)
    {
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
        var command = new RemoveRoomCommand()
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
        var command = new EnableRoomCommand()
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
        var command = new DisableRoomCommand()
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
        Console.WriteLine(request.Description);
        var command = new UpdateRoomCommand()
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