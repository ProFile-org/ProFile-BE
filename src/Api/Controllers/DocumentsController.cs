using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Departments.Commands.CreateDepartment;
using Application.Documents.Commands.ImportDocument;
using Application.Documents.Queries.GetDocumentsByTitle;
using Application.Users.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class DocumentsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Result<DocumentDto>>> ImportDocument([FromBody] ImportDocumentCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<DocumentDto>.Succeed(result));
    }

    [HttpGet]
    public async Task<ActionResult<Result<PaginatedList<DocumentDto>>>> GetDocumentsByTitle(
        [FromQuery] GetDocumentsByTitleQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<DocumentDto>>.Succeed(result));
    }
}