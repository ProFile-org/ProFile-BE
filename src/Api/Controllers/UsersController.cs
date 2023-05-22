using Application.Common.Models;
using Application.Users.Commands.CreateUser;
using Application.Users.Commands.DisableUser;
using Application.Users.Queries;
using Application.Users.Queries.GetUsersByName;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class UsersController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]    
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<UserDto>>> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<UserDto>.Succeed(result));
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<PaginatedList<UserDto>>>> GetUsersByName(string? searchTerm, int page, int size)
    {
        var query = new GetUsersByNameQuery
        {
            SearchTerm = searchTerm,
            Page = page,
            Size = size
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<UserDto>>.Succeed(result));
    }

    [HttpPost("disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<UserDto>>> DisableUser([FromBody] DisableUserCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<UserDto>.Succeed(result));
    }
}
