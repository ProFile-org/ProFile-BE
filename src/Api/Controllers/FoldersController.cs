using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Folders.Commands.AddFolder;
using Application.Folders.Commands.DisableFolder;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class FoldersController : ApiControllerBase
{
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

    [HttpPost("disable")]
    public async Task<ActionResult<Result<FolderDto>>> DisableFolder([FromBody] DisableFolderCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<FolderDto>.Succeed(result));
    }
}