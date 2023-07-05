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
}