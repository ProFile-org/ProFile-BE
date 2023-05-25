using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Application.Rooms.Commands.CreateRoom;
using Application.Rooms.Commands.RemoveRoom;
using Application.Rooms.Queries.GetEmptyContainersPaginated;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class RoomsController : ApiControllerBase
{
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<RoomDto>>> AddRoom(CreateRoomCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<RoomDto>.Succeed(result));
    }

    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPost("empty-containers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedList<EmptyLockerDto>>> GetEmptyContainers(GetEmptyContainersPaginatedQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<EmptyLockerDto>>.Succeed(result));
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<RoomDto>>> RemoveRoom(RemoveRoomCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<RoomDto>.Succeed(result));
    }
}