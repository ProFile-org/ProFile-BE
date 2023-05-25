﻿using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Application.Lockers.Commands.AddLocker;
using Application.Lockers.Commands.DisableLocker;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class LockersController : ApiControllerBase
{
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<LockerDto>>> AddLocker([FromBody] AddLockerCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<LockerDto>.Succeed(result));
    }

    [HttpDelete]
    public async Task<ActionResult<Result<LockerDto>>> RemoveLocker([FromBody] DisableLockerCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<LockerDto>.Succeed(result));
    }
}
