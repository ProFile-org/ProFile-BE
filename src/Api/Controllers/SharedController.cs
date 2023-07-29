using Api.Controllers.Payload.Requests.Entries;
using Application.Common.Exceptions;
using Api.Controllers.Payload.Requests.Users;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Digital;
using Application.Entries.Commands;
using Application.Entries.Queries;
using Application.Identity;
using FluentValidation.Results;
using Application.Users.Queries;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class SharedController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public SharedController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Download a shared file
    /// </summary>
    /// <param name="entryId"></param>
    /// <returns>the download file</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet("entries/{entryId:guid}/file")]
    public async Task<ActionResult> DownloadSharedFile([FromRoute] Guid entryId)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var query = new DownloadSharedEntry.Query()
        {
            CurrentUser = currentUser,
            EntryId = entryId,
        };
        var result = await Mediator.Send(query);
        var content = new MemoryStream(result.FileData);
        HttpContext.Response.ContentType = result.FileType;
        return File(content, result.FileType, result.FileName);
    }

    /// <summary>
    /// Get all shared entries paginated
    /// </summary>
    /// <returns>a paginated list of EntryDto</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet("entries")]
    public async Task<ActionResult<PaginatedList<EntryDto>>> GetAll(
        [FromQuery] GetAllSharedEntriesPaginatedQueryParameters queryParameters)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var query = new GetAllSharedEntriesPaginated.Query()
        {
            CurrentUser = currentUser,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            EntryId = queryParameters.EntryId,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<EntryDto>>.Succeed(result)); 
    }


    /// <summary>
    ///  Upload a file or create a directory to a shared entry
    /// </summary>
    /// <returns>an EntryDto</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpPost("entries/{entryId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<EntryDto>>> CreateSharedEntry(
        [FromRoute] Guid entryId,
        [FromForm] UploadSharedEntryRequest request)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        if (request is { IsDirectory: true, File: not null })
        {
            throw new RequestValidationException(new List<ValidationFailure>()
            {
                new("File", "Cannot create a directory with a file.")
            });
        }

        if (request is { IsDirectory: false, File: null })
        {
            throw new RequestValidationException(new List<ValidationFailure>()
            {
                new("File", "Cannot upload with no files.")
            });
        }

        CreateSharedEntry.Command command;

        if (request.IsDirectory)
        {
            command = new CreateSharedEntry.Command()
            {
                Name = request.Name,
                CurrentUser = currentUser,
                EntryId = entryId,
                FileData = null,
                FileExtension = null,
                FileType = null,
                IsDirectory = true
            };
        }
        else
        {
            var fileData = new MemoryStream();
            await request.File!.CopyToAsync(fileData);
            var lastDotIndex = request.File.FileName.LastIndexOf(".", StringComparison.Ordinal);
            var extension =
                request.File.FileName.Substring(lastDotIndex + 1, request.File.FileName.Length - lastDotIndex - 1);
            command = new CreateSharedEntry.Command()
            {
                CurrentUser =  currentUser,
                EntryId = entryId,
                Name = request.Name,
                IsDirectory = false,
                FileType = request.File.ContentType,
                FileData = fileData,
                FileExtension = extension,
            };
        }
        var result = await Mediator.Send(command);
        return Ok(Result<EntryDto>.Succeed(result));
    }

    /// <summary>
    /// Get shared users from a shared entries paginated
    /// </summary>
    /// <returns>a paginated list of UserDto</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet("entries/{entryId:guid}/shared-users")]
    public async Task<ActionResult<PaginatedList<EntryPermissionDto>>> GetSharedUsersFromASharedEntryPaginated(
        [FromRoute] Guid entryId,
        [FromQuery] GetAllSharedUsersPaginatedQueryParameters queryParameters)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var query = new GetAllSharedUsersOfASharedEntryPaginated.Query()
        {
            CurrentUser = currentUser,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            SearchTerm = queryParameters.SearchTerm,
            EntryId = entryId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<EntryPermissionDto>>.Succeed(result)); 
    }


    /// <summary>
    /// Share an entry to a user
    /// </summary>
    /// <param name="entryId"></param>
    /// <returns>an EntryPermissionDto</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet("entries/{entryId}/permissions")]
    public async Task<ActionResult<Result<EntryPermissionDto>>> SharePermissions(
        [FromRoute] Guid entryId)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var query = new GetPermissions.Query
        {
            CurrentUser = currentUser,
            EntryId = entryId
        };
        var result = await Mediator.Send(query);
        return Ok(Result<EntryPermissionDto>.Succeed(result));
    }
    
    /// <summary>
    /// Get shared entry by Id 
    /// </summary>
    /// <returns>an EntryDto</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet("entries/{entryId:guid}")]
    public async Task<ActionResult<Result<EntryDto>>> GetSharedEntryById(
        [FromRoute] Guid entryId)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var query = new GetSharedEntryById.Query()
        {
            CurrentUser = currentUser,
            EntryId = entryId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<EntryDto>.Succeed(result)); 
    }
}