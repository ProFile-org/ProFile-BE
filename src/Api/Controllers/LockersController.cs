using Api.Controllers.Payload.Requests;
using Api.Controllers.Payload.Requests.Lockers;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Application.Lockers.Commands.AddLocker;
using Application.Lockers.Commands.DisableLocker;
using Application.Lockers.Commands.EnableLocker;
using Application.Lockers.Commands.RemoveLocker;
using Application.Lockers.Commands.UpdateLocker;
using Application.Lockers.Queries.GetAllLockersPaginated;
using Application.Lockers.Queries.GetLockerById;
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
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<LockerDto>>> GetById([FromRoute] Guid lockerId)
    {
        var query = new GetLockerByIdQuery()
        {
            LockerId = lockerId
        };
        var result = await Mediator.Send(query);
        return Ok(Result<LockerDto>.Succeed(result));
    }
   
    /// <summary>
    /// Get all lockers paginated
    /// </summary>
    /// <param name="queryParameters">Get all lockers query parameters</param>
    /// <returns>A paginated list of LockerDto</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<PaginatedList<LockerDto>>>> GetAllPaginated(
        [FromQuery] GetAllLockersPaginatedQueryParameters queryParameters)
    {
        var query = new GetAllLockersPaginatedQuery()
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
    
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<LockerDto>>> AddLocker([FromBody] AddLockerCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<LockerDto>.Succeed(result));
    }

    [HttpPut("disable")]
    public async Task<ActionResult<Result<LockerDto>>> DisableLocker([FromBody] DisableLockerCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<LockerDto>.Succeed(result));
    }

    [HttpPut("enable")]
    public async Task<ActionResult<Result<LockerDto>>> EnableLocker([FromBody] EnableLockerCommand command)
    {
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
        var command = new UpdateLockerCommand()
        {
            LockerId = lockerId,
            Name = request.Name,
            Description = request.Description,
            Capacity = request.Capacity
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
        var command = new RemoveLockerCommand()
        {
            LockerId = lockerId
        };
        var result = await Mediator.Send(command);
        return Ok(Result<LockerDto>.Succeed(result));
    }
}
