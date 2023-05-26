using Api.Controllers.Payload.Requests;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Application.Rooms.Commands.CreateRoom;
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
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<RoomDto>>> AddRoom(CreateRoomCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<RoomDto>.Succeed(result));
    }

    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPost("empty-containers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedList<EmptyLockerDto>>> GetEmptyContainers(GetEmptyContainersPaginatedQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<EmptyLockerDto>>.Succeed(result));
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<RoomDto>>> DisableRoom(DisableRoomCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<RoomDto>.Succeed(result));
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<RoomDto>>> RemoveRoom(RemoveRoomCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<RoomDto>.Succeed(result));
    }
    
    /// <summary>
    /// Enable a room
    /// </summary>
    /// <param name="command">Enable room details</param>
    /// <returns>A RoomDto of the enabled room</returns>
    [HttpPut("enable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<RoomDto>>> EnableRoom([FromBody] EnableRoomCommand command)
    {
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
        var command = new UpdateRoomCommand()
        {
            RoomId = roomId,
            Description = request.Description,
            Capacity = request.Capacity,
            StaffId = request.StaffId
        };
        var result = await Mediator.Send(command);
        return Ok(Result<RoomDto>.Succeed(result));
    }
    
    /// <summary>
    /// Get a room based on id
    /// </summary>
    /// <param name="roomId">Id of the room to be found</param>
    /// <returns>A RoomDto of the found room</returns>
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
    /// <param name="page">The page index</param>
    /// <param name="size">The size number</param>
    /// <param name="sortBy">Criteria</param>
    /// <param name="sortOrder">The order in which the rooms are sorted</param>
    /// <returns>A paginated list of rooms</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<PaginatedList<RoomDto>>>> GetAllPaginated(int? page, int? size, string? sortBy, string? sortOrder)
    {
        var query = new GetAllRoomPaginatedQuery()
        {
            Page = page,
            Size = size,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<RoomDto>>.Succeed(result));
    }
}