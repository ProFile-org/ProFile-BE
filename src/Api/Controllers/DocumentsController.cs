using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Departments.Commands.CreateDepartment;
using Application.Documents.Commands.ImportDocument;
using Application.Documents.Queries.GetAllDocumentsPaginated;
using Application.Documents.Queries.GetDocumentsByTitle;
using Application.Documents.Queries.GetDocumentTypes;
using Application.Users.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class DocumentsController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<DocumentDto>>> ImportDocument([FromBody] ImportDocumentCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(Result<DocumentDto>.Succeed(result));
    }

    [HttpGet("types")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<IEnumerable<string>>>> GetAllDocumentTypes()
    {
        var result = await Mediator.Send(new GetAllDocumentTypesQuery());
        return Ok(Result<IEnumerable<string>>.Succeed(result));
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<PaginatedList<DocumentItemDto>>>> GetAllDocuments(Guid? roomId, Guid? lockerId, Guid? folderId, int? page, int? size, string? sortBy, string? sortOrder)
    {
        var query = new GetAllDocumentsPaginatedQuery()
        {
            RoomId = roomId,
            LockerId = lockerId,
            FolderId = folderId,
            Page = page,
            Size = size,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<DocumentItemDto>>.Succeed(result));
    }

    [HttpGet("search-documents")]
    public async Task<ActionResult<Result<PaginatedList<DocumentDto>>>> GetDocumentsById(
        [FromQuery] GetDocumentsByTitleQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<DocumentDto>>.Succeed(result));
    }
}