using Api.Controllers.Payload.Requests;
using Api.Controllers.Payload.Requests.Users;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Logging;
using Application.Identity;
using Application.Users.Commands;
using Application.Users.Queries;
using Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff, IdentityData.Roles.Employee)]
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<UserDto>>> GetById([FromRoute] Guid userId)
    {
        var role = _currentUserService.GetRole();
        var userDepartmentId = _currentUserService.GetDepartmentId();
        var query = new GetUserById.Query()
        {
            UserRole = role,
            UserDepartmentId = userDepartmentId,
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
            DepartmentIds = queryParameters.DepartmentIds,
            Role = queryParameters.Role,
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
    /// Get all employees in the same department
    /// </summary>
    /// <param name="queryParameters">Query parameters</param>
    /// <returns>A list of UserDtos</returns>
    [RequiresRole(IdentityData.Roles.Staff, IdentityData.Roles.Employee)]
    [HttpGet("employees")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<PaginatedList<UserDto>>>> GetAllEmployeesPaginated(
        [FromQuery] GetAllEmployeesPaginatedQueryParameters queryParameters)
    {
        var departmentId = _currentUserService.GetDepartmentId();
        var query = new GetAllUsersPaginated.Query()
        {
            DepartmentIds = new []{ departmentId },
            Role = IdentityData.Roles.Employee,
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
        var currentUser = _currentUserService.GetCurrentUser();
        var command = new AddUser.Command()
        {
            CurrentUser = currentUser,
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
    /// Update a user
    /// </summary>
    /// <param name="userId">Id of the user to be updated</param>
    /// <param name="request">Update user details</param>
    /// <returns>A UserDto of the updated user</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPut("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<UserDto>>> Update(
        [FromRoute] Guid userId,
        [FromBody] UpdateUserRequest request)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var command = new UpdateUser.Command()
        {
            CurrentUser = currentUser,
            UserId = userId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Position = request.Position,
            Role = request.Role,
            IsActive = request.IsActive,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<UserDto>.Succeed(result));
    }
    
    /// <summary>
    /// Update a user
    /// </summary>
    /// <param name="request">Update user details</param>
    /// <returns>A UserDto of the updated user</returns>
    [RequiresRole(IdentityData.Roles.Staff, IdentityData.Roles.Employee)]
    [HttpPut("self")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<UserDto>>> UpdateSelf(
        [FromBody] UpdateSelfRequest request)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var command = new UpdateUser.Command()
        {
            CurrentUser = currentUser,
            UserId = currentUser.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Position = currentUser.Position,
            Role = currentUser.Role,
            IsActive = currentUser.IsActive,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<UserDto>.Succeed(result));
    }

    /// <summary>
    /// Get all user related logs paginated
    /// </summary>
    /// <param name="queryParameters">Get all users related logs query parameters</param>
    /// <returns>A paginated list of UserLogDto</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpGet("logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<PaginatedList<UserLogDto>>>> GetAllLogsPaginated(
        [FromQuery] GetAllLogsPaginatedQueryParameters queryParameters)
    {
        var query = new GetAllUserLogsPaginated.Query()
        {
            SearchTerm = queryParameters.SearchTerm,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            UserId = queryParameters.UserId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<UserLogDto>>.Succeed(result));
    }
}