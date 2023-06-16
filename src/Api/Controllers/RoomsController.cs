using Api.Controllers.Payload.Requests;
using Api.Controllers.Payload.Requests.Lockers;
using Api.Controllers.Payload.Requests.Rooms;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Logging;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Application.Rooms.Commands;
using Application.Rooms.Queries;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class RoomsController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public RoomsController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get a room by id
    /// </summary>
    /// <param name="roomId">Id of the room to be retrieved</param>
    /// <returns>A RoomDto of the retrieved room</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpGet("{roomId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<RoomDto>>> GetById(
        [FromRoute] Guid roomId)
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
    /// <param name="roomId"></param>
    /// <param name="queryParameters">Get empty containers paginated details</param>
    /// <returns>A paginated list of EmptyLockerDto</returns>
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPost("{roomId:guid}/empty-containers")]
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
    public async Task<ActionResult<Result<RoomDto>>> AddRoom(
        [FromBody] AddRoomRequest request)
    {
        var performingUserId = _currentUserService.GetId();
        var command = new AddRoom.Command()
        {
            PerformingUserId = performingUserId,
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
    public async Task<ActionResult<Result<RoomDto>>> RemoveRoom(
        [FromRoute] Guid roomId)
    {
        var command = new RemoveRoom.Command()
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
    public async Task<ActionResult<Result<RoomDto>>> Update(
        [FromRoute] Guid roomId,
        [FromBody] UpdateRoomRequest request)
    {
        var performingUserId = _currentUserService.GetId();
        var command = new UpdateRoom.Command()
        {
            PerformingUserId = performingUserId,
            RoomId = roomId,
            Name = request.Name,
            Description = request.Description,
            Capacity = request.Capacity,
            IsAvailable = request.IsAvailable,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<RoomDto>.Succeed(result));
    }

    /// <summary>
    /// Get all room logs paginated
    /// </summary>
    /// <param name="queryParameters"></param>
    /// <returns>A paginated list of RoomLogDto</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpGet("logs")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<PaginatedList<RoomLogDto>>>> GetAllLogsPaginated(
        [FromQuery] GetAllLogsPaginatedQueryParameters queryParameters)
    {
        var query = new GetAllRoomLogsPaginated.Query()
        {
            SearchTerm = queryParameters.SearchTerm,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<RoomLogDto>>.Succeed(result));
    }

    /// <summary>
    /// Get a room log by id 
    /// </summary>
    /// <param name="logId"></param>
    /// <returns>A RoomLogDto</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpGet("logs/{logId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<RoomLogDto>>> GetLogById(
        [FromRoute] Guid logId)
    {
        var query = new GetRoomLogById.Query()
        {
            LogId = logId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<RoomLogDto>.Succeed(result));
    }
}