using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Documents.Commands.ImportDocument;
using Application.Documents.Queries.GetAllDocumentsPaginated;
using Application.Documents.Queries.GetDocumentById;
using Application.Documents.Queries.GetDocumentTypes;
using Application.Identity;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class DocumentsController : ApiControllerBase
{
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
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

    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpGet("types")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<IEnumerable<string>>>> GetAllDocumentTypes()
    {
        var result = await Mediator.Send(new GetAllDocumentTypesQuery());
        return Ok(Result<IEnumerable<string>>.Succeed(result));
    }

    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<PaginatedList<DocumentDto>>>> GetAllDocuments(Guid? roomId, Guid? lockerId, Guid? folderId, int? page, int? size, string? sortBy, string? sortOrder)
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
        return Ok(Result<PaginatedList<DocumentDto>>.Succeed(result));
    }
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<DocumentDto>>> GetDocumentById(Guid id)
    {
        var query = new GetDocumentByIdQuery()
        {
            Id = id
        };
        var result = await Mediator.Send(query);
        return Ok(Result<DocumentDto>.Succeed(result));
    }
}