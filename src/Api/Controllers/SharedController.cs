using Api.Controllers.Payload.Requests.DigitalFile;
using Api.Controllers.Payload.Requests.Entries;
using Application.Common.Exceptions;
using Api.Controllers.Payload.Requests.Users;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Digital;
using Application.Digital.Commands;
using Application.Entries.Queries;
using Application.Identity;
using FluentValidation.Results;
using Application.Users.Queries;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;
using GetAllEntriesPaginatedQueryParameters = Api.Controllers.Payload.Requests.Entries.GetAllEntriesPaginatedQueryParameters;

namespace Api.Controllers;

public class SharedController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public SharedController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entryId"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <returns></returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet("entries")]
    public async Task<ActionResult<PaginatedList<EntryDto>>> GetAll(
        [FromQuery] GetAllEntriesPaginatedQueryParameters queryParameters)
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
    ///  upload a file or create a directory to a shared entry
    /// </summary>
    /// <returns>an EntryDto</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpPost("entries/{entryId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<EntryDto>>> UploadSharedEntry([
        FromRoute] Guid entryId,
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

        UploadSharedEntry.Command command;

        if (request.IsDirectory)
        {
            command = new UploadSharedEntry.Command()
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
            command = new UploadSharedEntry.Command()
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
    /// 
    /// </summary>
    /// <returns></returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet("entries/{entryId:guid}/shared-users")]
    public async Task<ActionResult<PaginatedList<UserDto>>> GetSharedUsersFromASharedEntryPaginated(
        [FromRoute] Guid entryId,
        [FromQuery] GetAllSharedUsersFromASharedEntryPaginatedQueryParameters queryParameters)
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
        return Ok(Result<PaginatedList<UserDto>>.Succeed(result)); 
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns> [RequiresRole(IdentityData.Roles.Employee)]
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
}

