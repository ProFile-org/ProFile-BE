using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Digital;
using Application.Digital.Commands;
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
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
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
}