using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Folders.Commands.AddFolder;
using Application.Folders.Commands.DisableFolder;
using Application.Folders.Commands.EnableFolder;
using Application.Folders.Commands.RemoveFolder;
using Application.Folders.Queries.GetFolderById;
using Application.Identity;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class FoldersController : ApiControllerBase
{
    /// <summary>
    /// Get a folder by id
    /// </summary>
    /// <param name="folderId">Id of the folder to be retrieved</param>
    /// <returns>A FolderDto of the retrieved folder</returns>
    [HttpGet("{folderId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<FolderDto>>> GetById([FromRoute] Guid folderId)
    {
        var query = new GetFolderByIdQuery()
        {
            FolderId = folderId
        };
        var result = await Mediator.Send(query);
        return Ok(Result<FolderDto>.Succeed(result));
    }
    
    /// <summary>
    /// Add a folder
    /// </summary>
    /// <param name="command">Add folder details</param>
    /// <returns>A FolderDto of the added folder</returns>
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
    /// Remove a folder
    /// </summary>
    /// <param name="command">Remove folder details</param>
    /// <returns>A FolderDto of the removed folder</returns>
    [HttpPut("disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]    
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<FolderDto>>> RemoveFolder([FromBody] RemoveFolderCommand command)
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