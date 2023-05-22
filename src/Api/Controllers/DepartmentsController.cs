using Application.Common.Models;
using Application.Departments.Commands.CreateDepartment;
using Application.Users.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class DepartmentsController : ApiControllerBase
{
    /// <summary>
    /// Create a department
    /// </summary>
    /// <param name="command">command parameter to create a department</param>
    /// <returns>Result[DepartmentDto]</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<DepartmentDto>>> CreateDepartment([FromBody] CreateDepartmentCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<DepartmentDto>.Succeed(result));
    }
}