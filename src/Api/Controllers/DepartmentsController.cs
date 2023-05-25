using Application.Common.Models;
using Application.Departments.Commands.CreateDepartment;
using Application.Departments.Queries.GetAllDepartments;
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
}