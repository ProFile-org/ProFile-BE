using Api.Controllers.Payload.Requests.Entries;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Digital;
using Application.Entries.Commands;
using Application.Entries.Queries;
using Application.Identity;
using FluentValidation.Results;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;
using GetAllEntriesPaginatedQueryParameters = Api.Controllers.Payload.Requests.Entries.GetAllEntriesPaginatedQueryParameters;

namespace Api.Controllers;

public class EntriesController  : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public EntriesController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }
    
    /// <summary>
    ///  Upload a file or create a directory
    /// </summary>
    /// <param name="request"></param>
    /// <returns>an EntryDto</returns>
    [RequiresRole(IdentityData.Roles.Staff, IdentityData.Roles.Employee)]
    [HttpPost]
    public async Task<ActionResult<Result<EntryDto>>> UploadEntry(
        [FromForm] UploadDigitalFileRequest request)
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

        CreateEntry.Command command;

        if (request.IsDirectory)
        {
            command = new CreateEntry.Command()
            {
                CurrentUser =  currentUser,
                Path = request.Path,
                Name = request.Name,
                IsDirectory = true,
                FileType = null,
                FileData = null,
                FileExtension = null,
            };
        }
        else
        {
            var fileData = new MemoryStream();
            await request.File!.CopyToAsync(fileData);
            var lastDotIndex = request.File.FileName.LastIndexOf(".", StringComparison.Ordinal);
            var extension =
                request.File.FileName.Substring(lastDotIndex + 1, request.File.FileName.Length - lastDotIndex - 1);
            command = new CreateEntry.Command()
            {
                CurrentUser =  currentUser,
                Path = request.Path,
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
    /// Update an entry
    /// </summary>
    /// <param name="entryId"></param>
    /// <param name="request"></param>
    /// <returns>an EntryDto</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpPut("{entryId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    
    public async Task<ActionResult<Result<EntryDto>>> Update(
        [FromRoute] Guid entryId, 
        [FromBody] UpdateEntryRequest request)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var command = new UpdateEntry.Command()
        {
            Name = request.Name,
            EntryId = entryId,
            CurrentUser = currentUser
        };

        var result = await Mediator.Send(command);
        return Ok(Result<EntryDto>.Succeed(result));
    }
    

    /// <summary>
    /// Get all entries paginated
    /// </summary>
    /// <returns>a paginated list of EntryDto</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<PaginatedList<EntryDto>>>> GetAllPaginated(
        [FromQuery] GetAllEntriesPaginatedQueryParameters queryParameters )
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var query = new GetAllEntriesPaginated.Query()
        {
            CurrentUser = currentUser,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            EntryPath = queryParameters.EntryPath,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder
        };

        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<EntryDto>>.Succeed(result));
    }
    
    
    /// <summary>
    /// Get an entry by Id
    /// </summary>
    /// <param name="entryId"></param>
    /// <returns>an EntryDto</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet("{entryId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<EntryDto>>> GetById([FromRoute] Guid entryId)
    {
        var query = new GetEntryById.Query()
        {
            EntryId = entryId
        };

        var result = await Mediator.Send(query);
        return Ok(Result<EntryDto>.Succeed(result));
    }
    
    /// <summary>
    /// Share an entry
    /// </summary>
    /// <param name="request"></param>
    /// <returns>an EntryPermissionDto</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpPut("{entryId:guid}/permissions")]
    public async Task<ActionResult<Result<EntryPermissionDto>>> ManagePermission(
        [FromRoute] Guid entryId,
        [FromBody] ShareEntryPermissionRequest request)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var command = new ShareEntry.Command
        {
            CurrentUser = currentUser,
            EntryId = entryId,
            UserId = request.UserId,
            ExpiryDate = request.ExpiryDate,
            CanView = request.CanView,
            CanEdit = request.CanEdit,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<EntryPermissionDto>.Succeed(result));
    }

    /// <summary>
    /// Download a file
    /// </summary>
    /// <param name="entryId"></param>
    /// <returns>The download file</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet("{entryId:guid}/file")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DownloadFile([FromRoute] Guid entryId)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var command = new DownloadDigitalFile.Command()
        {
            CurrentUser = currentUser,
            EntryId = entryId
        };

        var result = await Mediator.Send(command);
        HttpContext.Response.ContentType = result.FileType;
        return File(result.Content, result.FileType, result.FileName);
    }
}