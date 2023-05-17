using Application.Common.Models;
using Application.Users.Commands.CreateUserCommand;
using Application.Users.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/v1/[controller]")]
public class UsersController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Result<UserDto>>> CreateUser(CreateUserCommand command)
    {
        var result = await Mediator.Send(command);
        return Created("", Result<UserDto>.Succeed(result));
    }
}
