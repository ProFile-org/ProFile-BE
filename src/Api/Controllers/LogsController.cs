using Api.Controllers.Payload.Requests;
using Application.Common.Models;
using Application.Common.Models.Dtos;
using Application.Identity;
using Application.Logs.Queries;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class LogsController : ApiControllerBase
{    
    /// <summary>
    /// Get logs paginated
    /// </summary>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<LogDto>>> GetAllPaginated(
        [FromQuery] GetAllLogsPaginatedQueryParameters queryParameters)
    {
        var query = new GetAllLogsPaginated.Query()
        {
            ObjectId = queryParameters.ObjectId,
            ObjectType = queryParameters.ObjectType,
            SearchTerm = queryParameters.SearchTerm,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<LogDto>>.Succeed(result));
    }
}