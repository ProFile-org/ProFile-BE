using Api.Controllers.Payload.Requests;
using Application.Common.Models;
using Application.Departments.Commands.CreateDepartment;
using Application.Departments.Commands.DeleteDepartment;
using Application.Departments.Commands.UpdateDepartment;
using Application.Departments.Queries.GetAllDepartments;
using Application.Departments.Queries.GetDepartmentById;
using Application.Identity;
using Application.Users.Queries;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class DepartmentsController : ApiControllerBase
{
    /// <summary>
    /// Create a department
    /// </summary>
    /// <param name="command">command parameter to create a department</param>
    /// <returns>Result[DepartmentDto]</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<DepartmentDto>>> CreateDepartment([FromBody] CreateDepartmentCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<DepartmentDto>.Succeed(result));
    }

    /// <summary>
    /// Get all documents
    /// </summary>
    /// <returns>a Result of an IEnumerable of DepartmentDto</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<IEnumerable<DepartmentDto>>>> GetAllDepartments()
    {
        var result = await Mediator.Send(new GetAllDepartmentsQuery());
        return Ok(Result<IEnumerable<DepartmentDto>>.Succeed(result));
    }
    
    /// <summary>
    /// Get back a department based on its id
    /// </summary>
    /// <param name="departmentId">id of the department to be retrieved</param>
    /// <returns>A DepartmentDto of the retrieved department</returns>
    [HttpGet("{departmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<DepartmentDto>>> GetDepartmentById([FromRoute] Guid departmentId)
    {
        var query = new GetDepartmentByIdQuery()
        {
            DepartmentId = departmentId
        };
        var result = await Mediator.Send(query);
        return Ok(Result<DepartmentDto>.Succeed(result));
    }
    
    /// <summary>
    /// Update a department
    /// </summary>
    /// <param name="departmentId">Id of the department to be updated</param>
    /// <param name="request">Update department details</param>
    /// <returns>A DepartmentDto of the updated department</returns>
    [HttpPut("{departmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<DepartmentDto>>> UpdateDepartment([FromRoute] Guid departmentId, [FromBody] UpdateDepartmentRequest request)
    {
        var command = new UpdateDepartmentCommand()
        {
            DepartmentId = departmentId,
            Name = request.Name
        };
        var result = await Mediator.Send(command);
        return Ok(Result<DepartmentDto>.Succeed(result));
    }
    
    /// <summary>
    /// Delete a department
    /// </summary>
    /// <param name="departmentId">Id of the department to be deleted</param>
    /// <returns>A DepartmentDto of the deleted department</returns>
    [HttpDelete("{departmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<DepartmentDto>>> DeleteDepartment([FromRoute] Guid departmentId)
    {
        var command = new DeleteDepartmentCommand()
        {
            DepartmentId = departmentId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<DepartmentDto>.Succeed(result));
    }
}