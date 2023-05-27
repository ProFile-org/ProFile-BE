using Api.Controllers.Payload.Requests.Folders;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;
using FolderCommands = Application.Folders.Commands;
using FolderQueries = Application.Folders.Queries;

namespace Api.Controllers;

public class FoldersController : ApiControllerBase
{
    /// <summary>
    /// Get a folder by id
    /// </summary>
    /// <param name="folderId">Id of the folder to be retrieved</param>
    /// <returns>A FolderDto of the retrieved folder</returns>
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpGet("{folderId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<FolderDto>>> GetById([FromRoute] Guid folderId)
    {
        var query = new FolderQueries.GetById.Query()
        {
            FolderId = folderId
        };
        var result = await Mediator.Send(query);
        return Ok(Result<FolderDto>.Succeed(result));
    }
    
    /// <summary>
    /// Get all folders paginated
    /// </summary>
    /// <param name="queryParameters">Get all folders paginated query parameters</param>
    /// <returns>A paginated list of FolderDto</returns>
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<PaginatedList<FolderDto>>>> GetAllPaginated(
        [FromQuery] GetAllFoldersPaginatedQueryParameters queryParameters)
    {
        var query = new FolderQueries.GetAllPaginated.Query()
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
    /// <param name="request">Add folder details</param>
    /// <returns>A FolderDto of the added folder</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<FolderDto>>> AddFolder([FromBody] AddFolderRequest request)
    {
        var command = new FolderCommands.Add.Command()
        {
            Name = request.Name,
            Description = request.Description,
            Capacity = request.Capacity,
            LockerId = request.LockerId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<FolderDto>.Succeed(result));
    }
    
    /// <summary>
    /// Remove a folder
    /// </summary>
    /// <param name="folderId">Id of the folder to be removed</param>
    /// <returns>A FolderDto of the removed folder</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpDelete("{folderId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]    
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<FolderDto>>> RemoveFolder([FromRoute] Guid folderId)
    {
        var command = new FolderCommands.Remove.Command()
        {
            FolderId = folderId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<FolderDto>.Succeed(result));
    }
    
    /// <summary>
    /// Enable a folder
    /// </summary>
    /// <param name="folderId">Id of the folder to be enabled</param>
    /// <returns>A FolderDto of the enabled folder</returns>
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpPut("enable/{folderId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<FolderDto>>> EnableFolder([FromRoute] Guid folderId)
    {
        var command = new FolderCommands.Enable.Command()
        {
            FolderId = folderId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<FolderDto>.Succeed(result));
    }

    /// <summary>
    /// Disable a folder
    /// </summary>
    /// <param name="folderId">Id of the disabled folder</param>
    /// <returns>A FolderDto of the disabled folder</returns>
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpPut("disable/{folderId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<FolderDto>>> DisableFolder([FromRoute] Guid folderId)
    {
        var command = new FolderCommands.Disable.Command()
        {
            FolderId = folderId
        };
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
        var command = new FolderCommands.Update.Command()
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