﻿using Api.Controllers.Payload.Requests.Lockers;
using Application.Common.Interfaces;
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
    private readonly ICurrentUserService _currentUserService;

    public LockersController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get a locker by id
    /// </summary>
    /// <param name="lockerId">Id of the locker to be retrieved</param>
    /// <returns>A LockerDto of the retrieved locker</returns>
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpGet("{lockerId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<LockerDto>>> GetById(
        [FromRoute] Guid lockerId)
    {
        var currentUserRole = _currentUserService.GetRole();
        var staffRoomId = _currentUserService.GetCurrentRoomForStaff();
        var query = new GetLockerById.Query()
        {
            CurrentUserRole = currentUserRole,
            CurrentStaffRoomId = staffRoomId,
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
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<PaginatedList<LockerDto>>>> GetAllPaginated(
        [FromQuery] GetAllLockersPaginatedQueryParameters queryParameters)
    {
        var currentUserId = _currentUserService.GetId();
        var currentUserRole = _currentUserService.GetRole();
        var currentUserDepartmentId = _currentUserService.GetDepartmentId();
        var query = new GetAllLockersPaginated.Query()
        {
            CurrentUserId = currentUserId,
            CurrentUserRole = currentUserRole,
            CurrentUserDepartmentId = currentUserDepartmentId,
            RoomId = queryParameters.RoomId,
            SearchTerm = queryParameters.SearchTerm,
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
    public async Task<ActionResult<Result<LockerDto>>> Add(
        [FromBody] AddLockerRequest request)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var command = new AddLocker.Command()
        {
            CurrentUser = currentUser,
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
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpDelete("{lockerId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<LockerDto>>> Remove([FromRoute] Guid lockerId)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var command = new RemoveLocker.Command()
        {
            CurrentUser = currentUser,
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
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpPut("{lockerId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<LockerDto>>> Update(
        [FromRoute] Guid lockerId,
        [FromBody] UpdateLockerRequest request)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var command = new UpdateLocker.Command()
        {
            CurrentUser = currentUser,
            LockerId = lockerId,
            Name = request.Name,
            Description = request.Description,
            Capacity = request.Capacity,
            IsAvailable = request.IsAvailable,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<LockerDto>.Succeed(result));
    }
}
