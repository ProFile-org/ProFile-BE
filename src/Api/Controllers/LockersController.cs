using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Lockers.Commands.AddLocker;
using Application.Lockers.Commands.DisableLocker;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class LockersController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Result<LockerDto>>> AddLocker([FromBody] AddLockerCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<LockerDto>.Succeed(result));
    }

    [HttpPost("remove")]
    public async Task<ActionResult<Result<LockerDto>>> RemoveLocker([FromBody] DisableLockerCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<LockerDto>.Succeed(result));
    }
}