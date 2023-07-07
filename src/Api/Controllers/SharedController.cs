using Api.Controllers.Payload.Requests.Entries;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Digital;
using Application.Entries.Queries;
using Application.Identity;
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
}