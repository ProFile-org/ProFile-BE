﻿using Api.Controllers.Payload.Requests.Lockers;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Application.Lockers.Commands;
using Application.Lockers.Queries;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class LockersController : ApiControllerBase
{
    /// <summary>
    /// Get a locker by id
    /// </summary>
    /// <param name="lockerId">Id of the locker to be retrieved</param>
    /// <returns>A LockerDto of the retrieved locker</returns>
    [HttpGet("{lockerId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<LockerDto>>> GetById([FromRoute] Guid lockerId)
    {
        var query = new GetLockerById.Query()
        {
            LockerId = lockerId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<LockerDto>.Succeed(result));
    }
   
    /// <summary>
    /// Get all lockers paginated
    /// </summary>
    /// <param name="queryParameters">Get all lockers paginated query parameters</param>
    /// <returns>A paginated list of LockerDto</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<PaginatedList<LockerDto>>>> GetAllPaginated(
        [FromQuery] GetAllLockersPaginatedQueryParameters queryParameters)
    {
        var query = new GetAllLockersPaginated.Query()
        {
            RoomId = queryParameters.RoomId,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<LockerDto>>.Succeed(result));
    }
    
    /// <summary>
    /// Add a locker
    /// </summary>
    /// <param name="request">Add locker details</param>
    /// <returns>A LockerDto of the added locker</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<LockerDto>>> Add([FromBody] AddLockerRequest request)
    {
        var command = new AddLocker.Command()
        {
            Name = request.Name,
            Description = request.Description,
            Capacity = request.Capacity,
            RoomId = request.RoomId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<LockerDto>.Succeed(result));
    }

    /// <summary>
    /// Remove a locker
    /// </summary>
    /// <param name="lockerId">Id of the locker to be removed</param>
    /// <returns>A LockerDto of the removed locker</returns>
    [HttpDelete("{lockerId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<LockerDto>>> Remove([FromRoute] Guid lockerId)
    {
        var command = new RemoveLocker.Command()
        {
            LockerId = lockerId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<LockerDto>.Succeed(result));
    }

    /// <summary>
    /// Enable a locker
    /// </summary>
    /// <param name="lockerId">Id of the locker to be enabled</param>
    /// <returns>A LockerDto of the enabled locker</returns>
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpPut("enable/{lockerId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<LockerDto>>> Enable([FromRoute] Guid lockerId)
    {
        var command = new EnableLocker.Command()
        {
            LockerId = lockerId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<LockerDto>.Succeed(result));
    }
    
    /// <summary>
    /// Disable a locker
    /// </summary>
    /// <param name="lockerId">Id of the locker to be disabled</param>
    /// <returns>A LockerDto of the disabled locker</returns>
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpPut("disable/{lockerId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<LockerDto>>> Disable([FromRoute] Guid lockerId)
    {
        var command = new DisableLocker.Command()
        {
            LockerId = lockerId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<LockerDto>.Succeed(result));
    }

    /// <summary>
    /// Update a locker
    /// </summary>
    /// <param name="lockerId">Id of the locker to be updated</param>
    /// <param name="request">Update locker details</param>
    /// <returns>A LockerDto of the updated locker</returns>
    [HttpPut("{lockerId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<LockerDto>>> Update([FromRoute] Guid lockerId, [FromBody] UpdateLockerRequest request)
    {
        var command = new UpdateLocker.Command()
        {
            LockerId = lockerId,
            Name = request.Name,
            Description = request.Description,
            Capacity = request.Capacity,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<LockerDto>.Succeed(result));
    }
}
