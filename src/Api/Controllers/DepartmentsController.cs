using Api.Controllers.Payload.Requests.Departments;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos;
using Application.Common.Models.Dtos.Physical;
using Application.Departments.Commands;
using Application.Departments.Queries;
using Application.Identity;
using Application.Rooms.Queries;
using Application.Users.Queries;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class DepartmentsController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public DepartmentsController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get a department by it id
    /// </summary>
    /// <param name="departmentId">id of the department to be retrieved</param>
    /// <returns>A DepartmentDto of the retrieved department</returns>
    [RequiresRole(
        IdentityData.Roles.Admin,
        IdentityData.Roles.Staff,
        IdentityData.Roles.Employee)]
    [HttpGet("{departmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<DepartmentDto>>> GetById(
        [FromRoute] Guid departmentId)
    {
        var role = _currentUserService.GetRole();
        var userDepartmentId = _currentUserService.GetDepartmentId();
        var query = new GetDepartmentById.Query()
        {
            UserRole = role,
            UserDepartmentId = userDepartmentId,
            DepartmentId = departmentId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<DepartmentDto>.Succeed(result));
    }
    
    /// <summary>
    /// Get rooms based on department id
    /// </summary>
    /// <param name="departmentId">id of the department to be retrieved</param>
    /// <returns>A DepartmentDto of the retrieved department</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpGet("{departmentId:guid}/rooms")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<ItemsResult<RoomDto>>>> GetRoomByDepartmentId(
        [FromRoute] Guid departmentId)
    {
        var currentUserRole = _currentUserService.GetRole();
        var currentUserDepartmentId = _currentUserService.GetDepartmentId();
        var query = new GetRoomByDepartmentId.Query()
        {
            CurrentUserRole = currentUserRole,
            CurrentUserDepartmentId = currentUserDepartmentId,
            DepartmentId = departmentId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<ItemsResult<RoomDto>>.Succeed(result));
    }
    
    /// <summary>
    /// Get all documents
    /// </summary>
    /// <returns>A list of DocumentDto</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<IEnumerable<DepartmentDto>>>> GetAll()
    {
        var result = await Mediator.Send(new GetAllDepartments.Query());
        return Ok(Result<IEnumerable<DepartmentDto>>.Succeed(result));
    }
    
    /// <summary>
    /// Add a department
    /// </summary>
    /// <param name="request">Add department details</param>
    /// <returns>A DepartmentDto of the the added department</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<DepartmentDto>>> Add(
        [FromBody] AddDepartmentRequest request)
    {
        var command = new AddDepartment.Command()
        {
            Name = request.Name,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<DepartmentDto>.Succeed(result));
    }

    /// <summary>
    /// Delete a department
    /// </summary>
    /// <param name="departmentId">Id of the department to be deleted</param>
    /// <returns>A DepartmentDto of the deleted department</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpDelete("{departmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<DepartmentDto>>> Delete([FromRoute] Guid departmentId)
    {
        var command = new DeleteDepartment.Command()
        {
            DepartmentId = departmentId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<DepartmentDto>.Succeed(result));
    }
}