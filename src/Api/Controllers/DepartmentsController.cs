using Application.Common.Models;
using Application.Departments.Commands.CreateDepartment;
using Application.Users.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class DepartmentsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Result<DepartmentDto>>> CreateDepartment([FromBody] CreateDepartmentCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<DepartmentDto>.Succeed(result));
    }
}