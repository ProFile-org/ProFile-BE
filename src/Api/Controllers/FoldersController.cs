using Api.Controllers.Payload.Requests;
using Api.Controllers.Payload.Requests.Folders;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Logging;
using Application.Common.Models.Dtos.Physical;
using Application.Folders.Commands;
using Application.Folders.Queries;
using Application.Identity;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class FoldersController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public FoldersController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

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
        var currentUserRole = _currentUserService.GetRole();
        var staffRoomId = _currentUserService.GetCurrentRoomForStaff();
        var query = new GetFolderById.Query()
        {
            CurrentUserRole = currentUserRole,
            CurrentStaffRoomId = staffRoomId,
            FolderId = folderId,
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
        var currentUserRole = _currentUserService.GetRole();
        var staffRoomId = _currentUserService.GetCurrentRoomForStaff();
        var query = new GetAllFoldersPaginated.Query()
        {
            CurrentUserRole = currentUserRole,
            CurrentStaffRoomId = staffRoomId,
            RoomId = queryParameters.RoomId,
            LockerId = queryParameters.LockerId,
            SearchTerm = queryParameters.SearchTerm,
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
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<FolderDto>>> AddFolder([FromBody] AddFolderRequest request)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var staffRoomId = _currentUserService.GetCurrentRoomForStaff();
        var command = new AddFolder.Command()
        {
            CurrentUser = currentUser,
            CurrentStaffRoomId = staffRoomId,
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
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpDelete("{folderId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]    
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<FolderDto>>> RemoveFolder([FromRoute] Guid folderId)
    {
        var currentUserRole = _currentUserService.GetRole();
        var staffRoomId = _currentUserService.GetCurrentRoomForStaff();
        var command = new RemoveFolder.Command()
        {
            CurrentUserRole = currentUserRole,
            CurrentStaffRoomId = staffRoomId,
            FolderId = folderId,
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
        var currentUser = _currentUserService.GetCurrentUser();
        var staffRoomId = _currentUserService.GetCurrentRoomForStaff();
        var command = new UpdateFolder.Command()
        {
            CurrentUser = currentUser,
            CurrentStaffRoomId = staffRoomId,
            FolderId = folderId,
            Name = request.Name,
            Description = request.Description,
            Capacity = request.Capacity,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<FolderDto>.Succeed(result));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpGet("logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<PaginatedList<FolderLogDto>>>> GetAllLogsPaginated(
        [FromQuery] GetAllLogsPaginatedQueryParameters queryParameters)
    {
        var query = new GetAllFolderLogsPaginated.Query()
        {
            SearchTerm = queryParameters.SearchTerm,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<FolderLogDto>>.Succeed(result));    
    }
}