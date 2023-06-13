using Api.Controllers.Payload.Requests.Documents;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Documents.Commands;
using Application.Documents.Queries;
using Application.Identity;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class DocumentsController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public DocumentsController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get a document by id
    /// </summary>
    /// <param name="documentId">Id of the document to be retrieved</param>
    /// <returns>A DocumentDto of the retrieved document</returns>
    [HttpGet("{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<DocumentDto>>> GetById([FromRoute] Guid documentId)
    {
        var query = new GetDocumentById.Query()
        {
            DocumentId = documentId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<DocumentDto>.Succeed(result));
    }

    /// <summary>
    /// Get all documents paginated
    /// </summary>
    /// <param name="queryParameters">Get all documents query parameters</param>
    /// <returns>A paginated list of DocumentDto</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<PaginatedList<DocumentDto>>>> GetAllPaginated(
        [FromQuery] GetAllDocumentsPaginatedQueryParameters queryParameters)
    {
        var query = new GetAllDocumentsPaginated.Query()
        {
            RoomId = queryParameters.RoomId,
            LockerId = queryParameters.LockerId,
            FolderId = queryParameters.FolderId,
            SearchTerm = queryParameters.SearchTerm,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<DocumentDto>>.Succeed(result));
    }
    
    /// <summary>
    /// Get all document types
    /// </summary>
    /// <returns>A list of document types</returns>
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpGet("types")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<IEnumerable<string>>>> GetAllDocumentTypes()
    {
        var result = await Mediator.Send(new GetAllDocumentTypes.Query());
        return Ok(Result<IEnumerable<string>>.Succeed(result));
    }
    
    /// <summary>
    /// Import a document
    /// </summary>
    /// <param name="request">Import document details</param>
    /// <returns>A DocumentDto of the imported document</returns>
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<DocumentDto>>> Import([FromBody] ImportDocumentRequest request)
    {
        var performingUserId = _currentUserService.GetId();
        var command = new ImportDocument.Command()
        {
            PerformingUserId = performingUserId,
            Title = request.Title,
            Description = request.Description,
            DocumentType = request.DocumentType,
            FolderId = request.FolderId,
            ImporterId = request.ImporterId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<DocumentDto>.Succeed(result));
    }
    
    /// <summary>
    /// Request to import a document
    /// </summary>
    /// <param name="request">Import document request details</param>
    /// <returns>A DocumentDto of the imported document</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpPost("request")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<DocumentDto>>> RequestImport([FromBody] RequestImportDocumentRequest request)
    {
        var performingUserId = _currentUserService.GetId();
        var command = new RequestImportDocument.Command()
        {
            Title = request.Title,
            Description = request.Description,
            DocumentType = request.DocumentType,
            IssuerId = performingUserId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<DocumentDto>.Succeed(result));
    }

    /// <summary>
    /// Update a document
    /// </summary>
    /// <param name="documentId">Id of the document to be updated</param>
    /// <param name="request">Update document details</param>
    /// <returns>A DocumentDto of the updated document</returns>
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpPut("{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<DocumentDto>>> Update([FromRoute] Guid documentId, [FromBody] UpdateDocumentRequest request)
    {
        var query = new UpdateDocument.Command()
        {
            DocumentId = documentId,
            Title = request.Title,
            Description = request.Description,
            DocumentType = request.DocumentType
        };
        var result = await Mediator.Send(query);
        return Ok(Result<DocumentDto>.Succeed(result));
    }

    /// <summary>
    /// Delete a document
    /// </summary>
    /// <param name="documentId">Id of the document to be deleted</param>
    /// <returns>A DocumentDto of the deleted document</returns>    
    [HttpDelete("{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<DocumentDto>>> Delete([FromRoute] Guid documentId)
    {
        var query = new DeleteDocument.Command()
        {
            DocumentId = documentId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<DocumentDto>.Succeed(result));
    }
    
    /// <summary>
    /// Get all documents of a user.
    /// </summary>
    /// <param name="userId">Id of the user</param>
    /// <param name="queryParameters">Query parameters</param>
    /// <returns>A list of DocumentDtos of the user.</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<DocumentDto>>> GetDocumentsOfUserPaginated([FromRoute] Guid userId,
        [FromQuery] GetDocumentsOfUserPaginatedQueryParameters queryParameters)
    {
        var query = new GetDocumentsOfUserPaginated.Query()
        {
            UserId = userId,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<DocumentDto>>.Succeed(result));
    }
}