using Api.Controllers.Payload.Requests.Users;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Application.Users.Commands.CreateUser;
using Application.Users.Commands.DisableUser;
using Application.Users.Commands.UpdateUser;
using Application.Users.Queries;
using Application.Users.Queries.GetAllUsersPaginated;
using Application.Users.Queries.GetUserById;
using Application.Users.Queries.GetUsersByName;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<Result<UserDto>>> GetUserById([FromRoute] Guid userId)
    {
        var query = new GetUserByIdQuery
        {
            UserId = userId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<UserDto>.Succeed(result));
    }
    
    /// <summary>
    /// Get all users paginated
    /// </summary>
    /// <param name="queryParameters">Get all users  query parameters</param>
    /// <returns>A paginated list of UserDto</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<PaginatedList<UserDto>>>> GetAllPaginated(
        [FromQuery] GetAllUsersPaginatedQueryParameters queryParameters)
    {
        var query = new GetAllUsersPaginatedQuery()
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
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]    
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<UserDto>>> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<UserDto>.Succeed(result));
    }

    [Authorize]
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<PaginatedList<UserDto>>>> GetUsersByName(string? searchTerm, int? page, int? size)
    {
        var query = new GetUsersByNameQuery
        {
            SearchTerm = searchTerm,
            Page = page,
            Size = size
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<UserDto>>.Succeed(result));
    }

    [HttpPost("disable")]
    [RequiresRole(IdentityData.Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<UserDto>>> DisableUser([FromBody] DisableUserCommand command)
    {
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
        var command = new UpdateUserCommand()
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
