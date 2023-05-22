using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Rooms.Commands.CreateRoom;
using Application.Rooms.Queries.GetEmptyContainersPaginated;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class RoomsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Result<RoomDto>>> AddRoom(CreateRoomCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<RoomDto>.Succeed(result));
    }

    [HttpGet("empty-containers")]
    public async Task<ActionResult<PaginatedList<EmptyLockerDto>>> GetEmptyContainers(GetEmptyContainersPaginatedQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<EmptyLockerDto>>.Succeed(result));
    }
}