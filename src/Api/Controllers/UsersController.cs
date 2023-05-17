using Application.Common.Models;
using Application.Users.Commands.CreateUser;
using Application.Users.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class UsersController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Result<UserDto>>> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<UserDto>.Succeed(result));
    }

    [HttpGet]
    public async Task<ActionResult<Result<IEnumerable<UserDto>>>> GetUserByName([FromBody] GetUserByNameQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(Result<IEnumerable<UserDto>>.Succeed(result));
    }
}
