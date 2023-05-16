using Application.Users.Commands.CreateUserCommand;
using Application.Users.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/v1/[controller]")]
public class UsersController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<UserDTO>> CreateUser(CreateUserCommand command)
    {
        return await Mediator.Send(command);
    }
}
