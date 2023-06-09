using Api.Controllers.Payload.Requests.Lockers;
using Application.Common.AccessControl.Models;
using Application.Common.AccessControl.Models.Operations;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Application.Lockers.Commands;
using Application.Lockers.Queries;
using Domain.Entities.Physical;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class LockersController : ApiControllerBase
{
    private readonly IPhysicalAccessControlList _physicalAccessControlList;
    private readonly ICurrentUserService _currentUserService;

    public LockersController(IPhysicalAccessControlList physicalAccessControlList, ICurrentUserService currentUserService)
    {
        _physicalAccessControlList = physicalAccessControlList;
        _currentUserService = currentUserService;
    }
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
        
        // admin and staff get a pass
        var role = _currentUserService.GetRole();
        if (role.Equals(IdentityData.Roles.Admin)
            || role.Equals(IdentityData.Roles.Staff))
        {
            return Ok(Result<LockerDto>.Succeed(result));
        }
        // check if the locker is not in user's department
        var departmentId = _currentUserService.GetDepartmentId();
        if (departmentId != result.Room.Department!.Id)
        {
            return Forbid();
        }
        
        // if it's public then pass
        if (!result.IsPrivate)
        {
            return Ok(Result<LockerDto>.Succeed(result));
        }
        
        var userId = _currentUserService.GetId();
        var resource = new PhysicalResource()
        {
            Id = lockerId,
            Type = ResourceType.Locker,
        };
        var principal = new PhysicalPrincipal()
        {
            UserId = userId,
        };
        
        if (!_physicalAccessControlList.IsGranted(resource, LockerOperation.Read, principal))
        {
            return Forbid();
        }
        
        return Ok(Result<LockerDto>.Succeed(result));
    }

    [HttpPost("{lockerId:guid}/share/{userId:guid}")]
    public IActionResult Share([FromRoute] Guid lockerId, [FromRoute] Guid userId)
    {
        var resource = new PhysicalResource()
        {
            Id = lockerId,
            Type = ResourceType.Locker,
        };
        var id = _currentUserService.GetId();
        var principal = new PhysicalPrincipal() { UserId = id };
        if (!_physicalAccessControlList.IsGranted(resource, LockerOperation.Share, principal))
        {
            return Forbid();
        }

        return Ok();
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
    public async Task<ActionResult<Result<LockerDto>>> Add([FromBody] AddLockerRequest request)
    {
        var command = new AddLocker.Command()
        {
            Name = request.Name,
            Description = request.Description,
            Capacity = request.Capacity,
            RoomId = request.RoomId,
            OwnerId = request.OwnerId,
        };
        var result = await Mediator.Send(command);
        var userId = _currentUserService.GetId();
        var resource = new PhysicalResource()
        {
            Id = result.Id,
            Type = ResourceType.Locker,
        };
        var principal = new PhysicalPrincipal()
        {
            UserId = userId,
        };
        _physicalAccessControlList.Grant(resource, LockerOperation.Read, principal);
        _physicalAccessControlList.Grant(resource, LockerOperation.Share, principal);
        _physicalAccessControlList.Grant(resource, LockerOperation.Update, principal);
        _physicalAccessControlList.Grant(resource, LockerOperation.Delete, principal);
        _physicalAccessControlList.Grant(resource, LockerOperation.AddFolder, principal);
        _physicalAccessControlList.Grant(resource, LockerOperation.RemoveFolder, principal);
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
