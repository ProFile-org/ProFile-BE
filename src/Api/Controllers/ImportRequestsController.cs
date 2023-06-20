using Api.Controllers.Payload.Requests.Documents;
using Api.Controllers.Payload.Requests.ImportRequests;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos;
using Application.Common.Models.Dtos.ImportDocument;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Application.ImportRequests.Commands;
using Application.ImportRequests.Queries;
using Domain.Enums;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/v1/documents/[controller]")]
public class ImportRequestsController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public ImportRequestsController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get an import request by id.
    /// </summary>
    /// <param name="importRequestId">Id of the request</param>>
    /// <returns>An ImportRequestDto of the request</returns>
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff, IdentityData.Roles.Employee)]
    [HttpGet("{importRequestId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedList<ImportRequestDto>>> GetImportRequestById(
        [FromRoute] Guid importRequestId)
    {
        var currentUserRole = _currentUserService.GetRole();
        var currentStaffRoomId = _currentUserService.GetCurrentRoomForStaff();
        var query = new GetImportRequestById.Query()
        {
            CurrentUserRole = currentUserRole,
            RequestId = importRequestId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<ImportRequestDto>.Succeed(result));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff, IdentityData.Roles.Employee)]
    [HttpGet]
    public async Task<ActionResult<Result<PaginatedList<ImportRequestDto>>>> GetAllImportRequestsPaginated(
        [FromQuery] GetAllImportRequestsPaginatedQueryParameters queryParameters)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var query = new GetAllImportRequestsPaginated.Query()
        {
            CurrentUser = currentUser,
            SearchTerm = queryParameters.SearchTerm,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<ImportRequestDto>>.Succeed(result));
    }

    /// <summary>
    /// Request to import a document
    /// </summary>
    /// <param name="request">Import document request details</param>
    /// <returns>A DocumentDto of the imported document</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<ImportRequestDto>>> RequestImport(
        [FromBody] RequestImportDocumentRequest request)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var command = new RequestImportDocument.Command()
        {
            Title = request.Title,
            Description = request.Description,
            DocumentType = request.DocumentType,
            IsPrivate = request.IsPrivate,
            Issuer = currentUser,
            RoomId = request.RoomId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<ImportRequestDto>.Succeed(result));
    }

    /// <summary>
    /// Approve a document request
    /// </summary>
    /// <param name="importRequestId">Id of the document to be approved</param>
    /// <param name="request"></param>
    /// <returns>A DocumentDto of the approved document</returns>
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPut("{importRequestId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<ImportRequestDto>>> ApproveOrReject(
        [FromRoute] Guid importRequestId,
        [FromBody] ApproveOrRejectImportRequest request)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var query = new ApproveOrRejectDocument.Command()
        {
            CurrentUser = currentUser,
            ImportRequestId = importRequestId,
            Reason = request.Reason,
            Decision = request.Decision,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<ImportRequestDto>.Succeed(result));
    }

    /// <summary>
    /// Assign a document to a folder
    /// </summary>
    /// <param name="importRequestId">Id of the document to be rejected</param>
    /// <param name="request"></param>
    /// <returns>A DocumentDto of the rejected document</returns>    
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPut("assign/{importRequestId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<ImportRequestDto>>> Assign(
        [FromRoute] Guid importRequestId,
        [FromBody] AssignDocumentToFolderRequest request)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var staffRoomId = _currentUserService.GetCurrentRoomForStaff();
        var query = new AssignDocument.Command()
        {
            CurrentUser = currentUser,
            StaffRoomId = staffRoomId,
            ImportRequestId = importRequestId,
            FolderId = request.FolderId,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<ImportRequestDto>.Succeed(result));
    }

    /// <summary>
    /// Checkin a document
    /// </summary>
    /// <param name="documentId"></param>
    /// <returns>A DocumentDto of the imported document</returns>
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPut("checkin/{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<DocumentDto>>> Checkin(
        [FromRoute] Guid documentId)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var command = new CheckinDocument.Command()
        {
            CurrentUser = currentUser,
            DocumentId = documentId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<DocumentDto>.Succeed(result));
    }
}