using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Folders.Commands.AddFolder;
using Application.Folders.Commands.DisableFolder;
using Application.Folders.Commands.EnableFolder;
using Application.Identity;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class FoldersController : ApiControllerBase
{
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]    
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<FolderDto>>> AddFolder([FromBody] AddFolderCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<FolderDto>.Succeed(result));
    }
    
    /// <summary>
    /// Enable a folder
    /// </summary>
    /// <param name="command">Enable folder details</param>
    /// <returns>A FolderDto of the enabled folder</returns>
    [HttpPut("enable")]
    [ProducesResponseType(StatusCodes.Status200OK)]    
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<FolderDto>>> EnableFolder([FromBody] EnableFolderCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<FolderDto>.Succeed(result));
    }

    /// <summary>
    /// Disable a folder
    /// </summary>
    /// <param name="command">Disable folder details</param>
    /// <returns>A FolderDto of the disabled folder</returns>
    [HttpPut("disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]    
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<FolderDto>>> DisableFolder([FromBody] DisableFolderCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<FolderDto>.Succeed(result));
    }
}