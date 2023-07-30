using Api.Controllers.Payload.Requests.Dashboard;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.DashBoard;
using Application.Dashboards.Queries;
using Application.Identity;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class DashboardController : ApiControllerBase
{
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPost("import-documents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<List<MetricResultDto>>>> GetImportDocuments(
        [FromBody] GetImportedDocumentsMetricsRequest request)
    {
        var query = new GetImportDocuments.Query()
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        var result = await Mediator.Send(query);
        return Ok(Result<List<MetricResultDto>>.Succeed(result));
    }

    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPost("logins")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<MetricResultDto>>> GetLoggedInUsers(
        [FromBody] DateTime date)
    {
        var query = new GetLoggedInUser.Query()
        {
            Date = date
        };
    
        var result = await Mediator.Send(query);
        return Ok(Result<MetricResultDto>.Succeed(result));
    }
    
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpPost("largest-drive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<LargestDriveDto>>> GetUserWithLargestDrive(
        [FromBody] DateTime date)
    {
        var query = new GetUserWithLargestDriveData.Query()
        {
            Date = date
        };
    
        var result = await Mediator.Send(query);
        return Ok(Result<LargestDriveDto>.Succeed(result));
    }
}