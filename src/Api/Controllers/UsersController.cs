using Api.Controllers.Payload.Requests.Users;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Identity;
using Application.Users.Commands;
using Application.Users.Queries;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Security;

namespace Api.Controllers;

public class UsersController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public UsersController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

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
        var query = new GetUserById.Query
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
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<PaginatedList<UserDto>>>> GetAllPaginated(
        [FromQuery] GetAllUsersPaginatedQueryParameters queryParameters)
    {
        var query = new GetAllUsersPaginated.Query()
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
    /// <param name="request">Add user details</param>
    /// <returns>A UserDto of the added user</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<UserDto>>> Add([FromBody] AddUserRequest request)
    {
        var performingUserId = _currentUserService.GetId();
        var command = new AddUser.Command()
        {
            PerformingUserId = performingUserId,
            Username = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role,
            Position = request.Position,
            DepartmentId = request.DepartmentId,
        };
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
        var command = new EnableUser.Command()
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
        var performingUserId = _currentUserService.GetId();
        var command = new DisableUser.Command()
        {
            PerformingUserId = performingUserId,
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
        var performingUserId = _currentUserService.GetId();
        var command = new UpdateUser.Command()
        {
            PerformingUserId = performingUserId,
            UserId = userId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Position = request.Position,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<UserDto>.Succeed(result));
    }

    /// <summary>
    /// Get all users with the "Employee" role of the current user's department.
    /// </summary>
    /// <param name="queryParameters">Query parameters</param>
    /// <returns>A list of UserDtos with the employee role of that department</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet("employees")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<PaginatedList<UserDto>>>> GetAllEmployeesPaginated(
        [FromQuery] GetAllEmployeesPaginatedQueryParameters queryParameters)
    {
        var performingUserDepartmentId = _currentUserService.GetDepartmentId();

        if (performingUserDepartmentId is null)
        {
            throw new KeyNotFoundException("User does not belong to a department.");
        }
        
        var query = new GetAllEmployeesPaginated.Query()
        {
            DepartmentId = performingUserDepartmentId.Value,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder,
        };

        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<UserDto>>.Succeed(result));
    }
}
