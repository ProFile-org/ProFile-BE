using Api.Controllers.Payload.Requests.Users;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Application.Users.Commands.Add;
using Application.Users.Commands.Disable;
using Application.Users.Commands.Update;
using Application.Users.Queries;
using Application.Users.Queries.GetAllPaginated;
using Application.Users.Queries.GetById;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserCommands = Application.Users.Commands;
using UserQueries = Application.Users.Queries;

namespace Api.Controllers;

public class UsersController : ApiControllerBase
{
    /// <summary>
    /// Get a user by id
    /// </summary>
    /// <param name="userId">Id of the user to be retrieved</param>
    /// <returns>A UserDto of the retrieved user</returns>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<UserDto>>> GetById([FromRoute] Guid userId)
    {
        var query = new UserQueries.GetById.Query
        {
            UserId = userId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<UserDto>.Succeed(result));
    }
    
    /// <summary>
    /// Get all users paginated
    /// </summary>
    /// <param name="queryParameters">Get all users query parameters</param>
    /// <returns>A paginated list of UserDto</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<PaginatedList<UserDto>>>> GetAllPaginated(
        [FromQuery] GetAllUsersPaginatedQueryParameters queryParameters)
    {
        var query = new UserQueries.GetAllPaginated.Query()
        {
            DepartmentId = queryParameters.DepartmentId,
            SearchTerm = queryParameters.SearchTerm,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<UserDto>>.Succeed(result));
    }
    
    /// <summary>
    /// Add a user
    /// </summary>
    /// <param name="command">Add user details</param>
    /// <returns>A UserDto of the added user</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<UserDto>>> Add([FromBody] UserCommands.Add.Command command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<UserDto>.Succeed(result));
    }

    /// <summary>
    /// Enable a user
    /// </summary>
    /// <param name="userId">Id of the user to be enabled</param>
    /// <returns>A UserDto of the enabled user</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPost("enable/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<UserDto>>> Enable([FromRoute] Guid userId)
    {
        var command = new UserCommands.Enable.Command()
        {
            UserId = userId
        };
        var result = await Mediator.Send(command);
        return Ok(Result<UserDto>.Succeed(result));
    }

    /// <summary>
    /// Disable a user
    /// </summary>
    /// <param name="userId">Id of the user to be disabled</param>
    /// <returns>A UserDto of the disabled user</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPut("disable/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<UserDto>>> Disable([FromRoute] Guid userId)
    {
        var command = new UserCommands.Disable.Command()
        {
            UserId = userId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<UserDto>.Succeed(result));
    }
    
    /// <summary>
    /// Update a user
    /// </summary>
    /// <param name="userId">Id of the user to be updated</param>
    /// <param name="request">Update user details</param>
    /// <returns>A UserDto of the updated user</returns>
    [HttpPut("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<UserDto>>> Update([FromRoute] Guid userId, [FromBody] UpdateUserRequest request)
    {
        var command = new UserCommands.Update.Command()
        {
            UserId = userId,
            Username = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role,
            Position = request.Position,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<UserDto>.Succeed(result));
    }
}
