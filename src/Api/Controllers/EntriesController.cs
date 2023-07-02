using Api.Controllers.Payload.Requests.DigitalFile;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Digital;
using Application.Digital.Commands;
using Application.Digital.Queries;
using Application.Identity;
using FluentValidation.Results;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class EntriesController  : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public EntriesController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [RequiresRole(IdentityData.Roles.Staff, IdentityData.Roles.Employee)]
    [HttpPost]
    public async Task<ActionResult<Result<EntryDto>>> UploadDigitalFile(
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

        UploadDigitalFile.Command command;

        if (request.IsDirectory)
        {
            command = new UploadDigitalFile.Command()
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
            command = new UploadDigitalFile.Command()
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
    /// 
    /// </summary>
    /// <returns></returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<PaginatedList<EntryDto>>>> GetAllPaginated(
        [FromQuery] GetAllEntriesPaginatedQueryParameters queryParameters )
    {
        var query = new GetAllEntriesPaginated.Query()
        {
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            EntryPath = queryParameters.EntryPath,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder
        };

        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<EntryDto>>.Succeed(result));
    }
}