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
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<UserDto>>> Add([FromBody] AddUserRequest request)
    {
        var performingUser = _currentUserService.GetCurrentUser();
        var command = new AddUser.Command()
        {
            PerformingUser = performingUser,
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
    [HttpPut("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<UserDto>>> Update([FromRoute] Guid userId, [FromBody] UpdateUserRequest request)
    {
        var performingUser = _currentUserService.GetCurrentUser();
        var command = new UpdateUser.Command()
        {
            PerformingUser = performingUser,
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
    /// Get all employees in the same department
    /// </summary>
    /// <param name="queryParameters">Query parameters</param>
    /// <returns>A list of UserDtos</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<PaginatedList<UserDto>>>> GetAllEmployeesPaginated(
        [FromQuery] GetAllEmployeesPaginatedQueryParameters queryParameters)
    {
        var departmentId = _currentUserService.GetDepartmentId();
        
        if (departmentId is null)
        {
            throw new KeyNotFoundException("User does not belong to a department.");
        }
        
        var query = new GetAllEmployeesPaginated.Query()
        {
            DepartmentId = departmentId.Value,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<UserDto>>.Succeed(result));
    }
    
    /// <summary>
    /// Get all user related logs paginated
    /// </summary>
    /// <param name="queryParameters">Get all users related logs query parameters</param>
    /// <returns>A paginated list of UserLogDto</returns>
    [HttpGet("logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<PaginatedList<UserLogDto>>>> GetAllUserLogs(
        [FromQuery] GetAllLogsPaginatedQueryParameters queryParameters)
    {
        var query = new GetAllUserLogsPaginated.Query()
        {
            SearchTerm = queryParameters.SearchTerm,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<UserLogDto>>.Succeed(result));
    }

    /// <summary>
    /// Get user related log by Id
    /// </summary>
    /// <param name="logId">Id of the logged user</param>
    /// <returns>UserLogDto of the logged user</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpGet("log/{logId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<UserLogDto>>> GetUserLogById([FromRoute] Guid logId)
    {
        var query = new GetUserLogById.Query()
        {
            LogId = logId
        };

        var result = await Mediator.Send(query);
        return Ok(Result<UserLogDto>.Succeed(result));
    }
}