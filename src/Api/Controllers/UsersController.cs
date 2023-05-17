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
        return Ok(Result<UserDto>.Succeed(result));
    }

    [HttpGet]
    public async Task<ActionResult<Result<IEnumerable<UserDto>>>> GetUserByName(GetUserByNameQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(Result<IEnumerable<UserDto>>.Succeed(result));
    }
}
