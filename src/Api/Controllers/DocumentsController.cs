using Api.Controllers.Payload.Requests.Documents;
using Application.Common.Extensions;
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
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff, IdentityData.Roles.Staff)]
    [HttpGet("{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<DocumentDto>>> GetById(
        [FromRoute] Guid documentId)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var query = new GetDocumentById.Query()
        {
            CurrentUser = currentUser,
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
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<PaginatedList<DocumentDto>>>> GetAllPaginated(
        [FromQuery] GetAllDocumentsPaginatedQueryParameters queryParameters)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        Guid? currentStaffRoomId = null;
        if (currentUser.Role.IsStaff())
        {
            currentStaffRoomId = _currentUserService.GetCurrentRoomForStaff();
        }
        var query = new GetAllDocumentsPaginated.Query()
        {
            CurrentUser = currentUser,
            CurrentStaffRoomId = currentStaffRoomId,
            UserId = queryParameters.UserId,
            RoomId = queryParameters.RoomId,
            LockerId = queryParameters.LockerId,
            FolderId = queryParameters.FolderId,
            SearchTerm = queryParameters.SearchTerm,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder,
            IsPrivate = queryParameters.IsPrivate,
            DocumentStatus = queryParameters.DocumentStatus,
            Role = queryParameters.UserRole,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<DocumentDto>>.Succeed(result));
    }

    /// <summary>
    /// Get all documents paginated
    /// </summary>
    /// <param name="queryParameters">Get all documents query parameters</param>
    /// <returns>A paginated list of DocumentDto</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet("employees")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<PaginatedList<DocumentDto>>>> GetAllForEmployeePaginated(
        [FromQuery] GetAllDocumentsForEmployeePaginatedQueryParameters queryParameters)
    {
        var currentUserId = _currentUserService.GetId();
        var currentUserDepartmentId = _currentUserService.GetDepartmentId();
        var query = new GetAllDocumentsForEmployeePaginated.Query()
        {
            CurrentUserId = currentUserId,
            CurrentUserDepartmentId = currentUserDepartmentId,
            SearchTerm = queryParameters.SearchTerm,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder,
            DocumentStatus = queryParameters.DocumentStatus,
            IsPrivate = queryParameters.IsPrivate,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<DocumentDto>>.Succeed(result));
    }

    /// <summary>
    /// Get all document types
    /// </summary>
    /// <returns>A list of document types</returns>
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff, IdentityData.Roles.Employee)]
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
    public async Task<ActionResult<Result<DocumentDto>>> Import(
        [FromBody] ImportDocumentRequest request)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var currentStaffRoomId = _currentUserService.GetCurrentRoomForStaff();
        if (currentUser.Department is null)
        {
            return Forbid();
        }
        var command = new ImportDocument.Command()
        {
            CurrentUser = currentUser,
            CurrentStaffRoomId = currentStaffRoomId,
            Title = request.Title,
            Description = request.Description,
            DocumentType = request.DocumentType,
            FolderId = request.FolderId,
            ImporterId = request.ImporterId,
            IsPrivate = request.IsPrivate,
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
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff, IdentityData.Roles.Employee)]
    [HttpPut("{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<DocumentDto>>> Update(
        [FromRoute] Guid documentId,
        [FromBody] UpdateDocumentRequest request)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        if (currentUser.Department is null)
        {
            return Forbid();
        }
        var query = new UpdateDocument.Command()
        {
            CurrentUser = currentUser,
            DocumentId = documentId,
            Title = request.Title,
            Description = request.Description,
            DocumentType = request.DocumentType,
            IsPrivate = request.IsPrivate,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<DocumentDto>.Succeed(result));
    }

    /// <summary>
    /// Delete a document
    /// </summary>
    /// <param name="documentId">Id of the document to be deleted</param>
    /// <returns>A DocumentDto of the deleted document</returns>
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpDelete("{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<DocumentDto>>> Delete(
        [FromRoute] Guid documentId)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        if (currentUser.Role.IsStaff()
            && currentUser.Department is null)
        {
            return Forbid();
        }
        var query = new DeleteDocument.Command()
        {
            CurrentUser = currentUser,
            DocumentId = documentId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<DocumentDto>.Succeed(result));
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
            CurrentUser = performingUser,
            DocumentId = documentId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PermissionDto>.Succeed(result));
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
    public async Task<ActionResult<Result<DocumentDto>>> SharePermissions(
        [FromRoute] Guid documentId,
        [FromBody] SharePermissionsRequest request)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var query = new ShareDocument.Command()
        {
            CurrentUser = currentUser,
            DocumentId = documentId,
            UserId = request.UserId,
            CanRead = request.CanRead,
            CanBorrow = request.CanBorrow,
            ExpiryDate = request.ExpiryDate,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<DocumentDto>.Succeed(result));
    }
    
    /// <summary>
    /// Download a file linked with a document
    /// </summary>
    /// <param name="documentId">Document id</param>
    /// <returns>File</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet("{documentId:guid}/file")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadFile(
        [FromRoute] Guid documentId)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var query = new DownloadDocumentFile.Query()
        {
            CurrentUser = currentUser,
            DocumentId = documentId,
        };
        var result = await Mediator.Send(query);
        var content = new MemoryStream(result.FileData);
        HttpContext.Response.ContentType = result.FileType;
        return File(content, result.FileType, result.FileName);
    }
}
