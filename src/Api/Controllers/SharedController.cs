using Api.Controllers.Payload.Requests.DigitalFile;
using Api.Controllers.Payload.Requests.Entries;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Digital;
using Application.Digital.Commands;
using Application.Entries.Queries;
using Application.Identity;
using FluentValidation.Results;
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
    /// 
    /// </summary>
    /// <returns></returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet("entries")]
    public async Task<ActionResult<PaginatedList<EntryDto>>> DownloadSharedFile(
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
}
