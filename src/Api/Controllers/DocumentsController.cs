using Api.Controllers.Payload.Requests.Documents;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos;
using Application.Common.Models.Dtos.ImportDocument;
using Application.Common.Models.Dtos.Physical;
using Application.Common.Models.Operations;
using Application.Documents.Commands;
using Application.Documents.Queries;
using Application.Identity;
using Domain.Enums;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class DocumentsController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPermissionManager _permissionManager;

    public DocumentsController(ICurrentUserService currentUserService, IPermissionManager permissionManager)
    {
        _currentUserService = currentUserService;
        _permissionManager = permissionManager;
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
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpGet("issued")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<PaginatedList<IssuedDocumentDto>>>> GetAllIssuedPaginated(
        [FromQuery] GetAllIssuedPaginatedQueryParameters queryParameters)
    {
        var departmentId = _currentUserService.GetCurrentDepartmentForStaff();
        var query = new GetAllIssuedDocumentsPaginated.Query()
        {
            DepartmentId = departmentId!.Value,
            SearchTerm = queryParameters.SearchTerm,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<IssuedDocumentDto>>.Succeed(result));
    }
    
    /// <summary>
    /// Get all documents paginated
    /// </summary>
    /// <param name="queryParameters">Get all documents query parameters</param>
    /// <returns>A paginated list of DocumentDto</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<PaginatedList<DocumentDto>>>> GetAllForAdminPaginated(
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
    /// Get all documents for staff paginated
    /// </summary>
    /// <param name="queryParameters">Get all documents for staff query parameters</param>
    /// <returns>A paginated list of DocumentDto</returns>
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpGet("staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<PaginatedList<DocumentDto>>>> GetAllForStaffPaginated(
        [FromQuery] GetAllDocumentsForStaffPaginatedQueryParameters queryParameters)
    {
        var roomId = _currentUserService.GetCurrentRoomForStaff();
        var query = new GetAllDocumentsPaginated.Query()
        {
            RoomId = roomId,
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
    public async Task<ActionResult<Result<IssuedDocumentDto>>> RequestImport([FromBody] RequestImportDocumentRequest request)
    {
        var performingUserId = _currentUserService.GetId();
        var command = new RequestImportDocument.Command()
        {
            Title = request.Title,
            Description = request.Description,
            DocumentType = request.DocumentType,
            IsPrivate = request.IsPrivate,
            IssuerId = performingUserId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<IssuedDocumentDto>.Succeed(result));
    }

    /// <summary>
    /// Checkin a document
    /// </summary>
    /// <param name="documentId"></param>
    /// <returns>A DocumentDto of the imported document</returns>
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPost("checkin{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<DocumentDto>>> Checkin(
        [FromRoute] Guid documentId)
    {
        var performingUserId = _currentUserService.GetId();
        var command = new CheckinDocument.Command()
        {
            PerformingUserId = performingUserId,
            DocumentId = documentId,
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
    public async Task<ActionResult<Result<DocumentDto>>> Update(
        [FromRoute] Guid documentId,
        [FromBody] UpdateDocumentRequest request)
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
    public async Task<ActionResult<Result<DocumentDto>>> Delete(
        [FromRoute] Guid documentId)
    {
        var query = new DeleteDocument.Command()
        {
            DocumentId = documentId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<DocumentDto>.Succeed(result));
    }

    /// <summary>
    /// Approve a document request
    /// </summary>
    /// <param name="documentId">Id of the document to be approved</param>
    /// <param name="request"></param>
    /// <returns>A DocumentDto of the approved document</returns>    
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPost("approve/{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<DocumentDto>>> Approve(
        [FromRoute] Guid documentId,
        [FromBody] ApproveImportRequest request)
    {
        var performingUserId = _currentUserService.GetId();
        var query = new ApproveDocument.Command()
        {
            PerformingUserId = performingUserId,
            DocumentId = documentId,
            Reason = request.Reason,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<DocumentDto>.Succeed(result));
    }

    /// <summary>
    /// Reject a document request
    /// </summary>
    /// <param name="documentId">Id of the document to be rejected</param>
    /// <param name="request"></param>
    /// <returns>A DocumentDto of the rejected document</returns>    
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPost("reject/{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<DocumentDto>>> Reject(
        [FromRoute] Guid documentId,
        [FromBody] RejectImportRequest request)
    {
        var performingUserId = _currentUserService.GetId();
        var query = new RejectDocument.Command()
        {
            PerformingUserId = performingUserId,
            DocumentId = documentId,
            Reason = request.Reason,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<DocumentDto>.Succeed(result));
    }
    
    /// <summary>
    /// Get a document request reason
    /// </summary>
    /// <param name="documentId">Id of the document to be rejected</param>
    /// <returns>A DocumentDto of the rejected document</returns>    
    [RequiresRole(IdentityData.Roles.Staff, IdentityData.Roles.Employee)]
    [HttpPost("reason/{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<ReasonDto>>> Reason(
        [FromRoute] Guid documentId)
    {
        var query = new GetDocumentReason.Query()
        {
            DocumentId = documentId,
            Type = RequestType.Import,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<ReasonDto>.Succeed(result));
    }

    /// <summary>
    /// Assign a document to 
    /// </summary>
    /// <param name="documentId">Id of the document to be rejected</param>
    /// <param name="request"></param>
    /// <returns>A DocumentDto of the rejected document</returns>    
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPost("assign/{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<DocumentDto>>> Assign(
        [FromRoute] Guid documentId, 
        [FromBody] AssignDocumentToFolderRequest request)
    {
        var performingUserId = _currentUserService.GetId();
        var query = new AssignDocument.Command()
        {
            PerformingUserId = performingUserId,
            DocumentId = documentId,
            FolderId = request.FolderId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<DocumentDto>.Succeed(result));
    }

    /// <summary>
    /// Share permissions for an employee of a specific document
    /// </summary>
    /// <param name="documentId">Id of the document</param>
    /// <param name="request"></param>
    /// <returns>A DocumentDto</returns>    
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpPost("{documentId:guid}/permissions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<bool>>> SharePermissions(
        [FromRoute] Guid documentId,
        [FromBody] SharePermissionsRequest request)
    {
        var performingUserId = _currentUserService.GetId();
        var query = new ShareDocument.Command()
        {
            PerformingUserId = performingUserId,
            DocumentId = documentId,
            UserIds = request.UserIds,
            CanRead = request.CanRead,
            CanBorrow = request.CanBorrow,
            ExpiryDate = request.ExpiryDate,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<bool>.Succeed(result));
    }
    
    /// <summary>
    /// Get permissions for an employee of a specific document
    /// </summary>
    /// <param name="documentId">Id of the document to be getting permissions from</param>
    /// <returns>A DocumentDto of the rejected document</returns>    
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet("{documentId:guid}/permissions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<PermissionDto>>> GetPermissions(
        [FromRoute] Guid documentId)
    {
        var performingUser = _currentUserService.GetCurrentUser();
        var query = new GetPermissions.Query()
        {
            PerformingUser = performingUser,
            DocumentId = documentId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PermissionDto>.Succeed(result));
    }
}