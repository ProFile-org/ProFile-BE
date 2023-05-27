using Api.Controllers.Payload.Requests.Folders;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Folders.Commands.AddFolder;
using Application.Folders.Commands.DisableFolder;
using Application.Folders.Commands.EnableFolder;
using Application.Folders.Commands.RemoveFolder;
using Application.Folders.Commands.UpdateFolder;
using Application.Folders.Queries.GetAllFoldersPaginated;
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
    /// Get all folders paginated
    /// </summary>
    /// <param name="queryParameters">Get all folders query parameters</param>
    /// <returns>A paginated list of FolderDto</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<PaginatedList<FolderDto>>>> GetAllPaginated(
        [FromQuery] GetAllFoldersPaginatedQueryParameters queryParameters)
    {
        var query = new GetAllFoldersPaginatedQuery()
        {
            RoomId = queryParameters.RoomId,
            LockerId = queryParameters.LockerId,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<FolderDto>>.Succeed(result));
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
    
    /// <summary>
    /// Update a folder
    /// </summary>
    /// <param name="folderId">Id of the folder to be updated</param>
    /// <param name="request">Update folder details</param>
    /// <returns>A FolderDto of the updated folder</returns>
    [HttpPut("{folderId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<FolderDto>>> Update([FromRoute] Guid folderId, [FromBody] UpdateFolderRequest request)
    {
        var command = new UpdateFolderCommand()
        {
            FolderId = folderId,
            Name = request.Name,
            Description = request.Description,
            Capacity = request.Capacity
        };
        var result = await Mediator.Send(command);
        return Ok(Result<FolderDto>.Succeed(result));
    }
}