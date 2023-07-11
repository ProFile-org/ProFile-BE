﻿using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Digital;
using Application.Entries.Commands;
using Application.Identity;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class BinController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public BinController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Move an entry to bin
    /// </summary>
    /// <param name="entryId"></param>
    /// <returns>an EntryDto</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpPost("entries")]
    public async Task<ActionResult<Result<EntryDto>>> MoveEntryToBin(
        [FromQuery] Guid entryId)
    {
        var currentUser = _currentUserService.GetCurrentUser();

        var command = new MoveEntryToBin.Command()
        {
            CurrentUser = currentUser,
            EntryId = entryId,
        };
            
        var result = await Mediator.Send(command);
        return Ok(Result<EntryDto>.Succeed(result));
    }

    /// <summary>
    /// Restore an entry from bin
    /// </summary>
    /// <param name="entryId"></param>
    /// <returns>an EntryDto</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpPut("entries/{entryId:guid}/restore")]
    public async Task<ActionResult<Result<EntryDto>>> RestoreBinEntry(
        [FromRoute] Guid entryId)
    {
        var currentUser = _currentUserService.GetCurrentUser();

        var command = new RestoreBinEntry.Command()
        {
            CurrentUser = currentUser,
            EntryId = entryId,
        };
            
        var result = await Mediator.Send(command);
        return Ok(Result<EntryDto>.Succeed(result));
    }
    
   
    /// <summary>
    /// Remove an entry from bin
    /// </summary>
    /// <param name="entryId"></param>
    /// <returns>an EntryDto</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpDelete("entries/{entryId:guid}")]
    public async Task<ActionResult<Result<EntryDto>>> DeleteBinEntry(
        [FromRoute] Guid entryId)
    {
        var currentUser = _currentUserService.GetCurrentUser();

        var command = new DeleteBinEntry.Command()
        {
            CurrentUser = currentUser,
            EntryId = entryId,
        };

        var result = await Mediator.Send(command);
        return Ok(Result<EntryDto>.Succeed(result));
    }
}