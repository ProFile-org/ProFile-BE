using Api.Controllers.Payload.Requests.Departments;
using Application.Common.Models;
using Application.Common.Models.Dtos;
using Application.Departments.Commands;
using Application.Departments.Queries;
using Application.Identity;
using Application.Users.Queries;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class DepartmentsController : ApiControllerBase
{
    /// <summary>
    /// Get back a department based on its id
    /// </summary>
    /// <param name="departmentId">id of the department to be retrieved</param>
    /// <returns>A DepartmentDto of the retrieved department</returns>
    [HttpGet("{departmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<DepartmentDto>>> GetById([FromRoute] Guid departmentId)
    {
        var query = new GetDepartmentById.Query()
        {
            DepartmentId = departmentId
        };
        var result = await Mediator.Send(query);
        return Ok(Result<DepartmentDto>.Succeed(result));
    }
    
    /// <summary>
    /// Get all documents
    /// </summary>
    /// <returns>A list of DocumentDto</returns>
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
    public async Task<ActionResult<Result<DepartmentDto>>> Add([FromBody] AddDepartmentRequest request)
    {
        var command = new AddDepartment.Command()
        {
            Name = request.Name,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<DepartmentDto>.Succeed(result));
    }

    /// <summary>
    /// Update a department
    /// </summary>
    /// <param name="departmentId">Id of the department to be updated</param>
    /// <param name="request">Update department details</param>
    /// <returns>A DepartmentDto of the updated department</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPut("{departmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<DepartmentDto>>> Update([FromRoute] Guid departmentId, [FromBody] UpdateDepartmentRequest request)
    {
        var command = new UpdateDepartment.Command()
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