using Api.Controllers.Payload.Requests.BinEntries;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Digital;
using Application.Digital.Queries;
using Application.Identity;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class BinController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public BinController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet("entries/{entryId:guid}")]
    public async Task<ActionResult<Result<EntryDto>>> GetBinEntryById(
        [FromRoute] Guid entryId)
    {
        var currentUser = _currentUserService.GetCurrentUser();

        var command = new GetBinEntryById.Query()
        {
            CurrentUser = currentUser,
            EntryId = entryId,
        };

        var result = await Mediator.Send(command);
        return Ok(Result<EntryDto>.Succeed(result));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet("entries")]
    public async Task<ActionResult<Result<EntryDto>>> GetAllBinEntriesPaginated(
        [FromQuery] GetAllBinEntriesPaginatedQueryParameters queryParameters)
    {
        var currentUser = _currentUserService.GetCurrentUser();

        var command = new GetAllBinEntriesPaginated.Query()
        {
            CurrentUser = currentUser,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            SearchTerm = queryParameters.SearchTerm,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder,
        };

        var result = await Mediator.Send(command);
        return Ok(Result<PaginatedList<EntryDto>>.Succeed(result));
    }
}