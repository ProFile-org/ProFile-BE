using Api.Controllers.Payload.Requests;
using Api.Controllers.Payload.Requests.Borrows;
using Application.Borrows.Commands;
using Application.Borrows.Queries;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Logging;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/v1/documents/[controller]")]
public class BorrowsController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    public BorrowsController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }
    /// <summary>
    /// Borrow a document request
    /// </summary>
    /// <param name="request">Borrow document details</param>
    /// <returns>A BorrowDto of the requested borrow</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<BorrowDto>>> BorrowDocument(
        [FromBody] BorrowDocumentRequest request)
    {
        var borrowerId = _currentUserService.GetCurrentUser().Id;
        var command = new BorrowDocument.Command()
        {
            BorrowerId = borrowerId,
            DocumentId = request.DocumentId,
            BorrowFrom = request.BorrowFrom,
            BorrowTo = request.BorrowTo,
            Reason = request.Reason,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<BorrowDto>.Succeed(result));
    }
    
    /// <summary>
    /// Get a borrow request by id
    /// </summary>
    /// <returns>A BorrowDto of the retrieved borrow</returns>
    [RequiresRole(IdentityData.Roles.Staff, IdentityData.Roles.Employee)]
    [HttpGet("{borrowId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<BorrowDto>>> GetById(
        [FromRoute] Guid borrowId)
    {
        var user = _currentUserService.GetCurrentUser();
        var command = new GetBorrowRequestById.Query()
        {
            BorrowId = borrowId,
            User = user,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<BorrowDto>.Succeed(result));
    }
    
    /// <summary>
    ///
    /// </summary>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff, IdentityData.Roles.Employee)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<PaginatedList<BorrowDto>>>> GetAllRequestsPaginated(
        [FromQuery] GetAllBorrowRequestsPaginatedQueryParameters queryParameters)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var command = new GetAllBorrowRequestsPaginated.Query()
        {
            CurrentUser = currentUser,
            RoomId = queryParameters.RoomId,
            EmployeeId = queryParameters.EmployeeId,
            DocumentId = queryParameters.DocumentId,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<PaginatedList<BorrowDto>>.Succeed(result));
    }

    /// <summary>
    /// Approve or Reject a borrow request
    /// </summary>
    /// <param name="borrowId">Id of the borrow request to be approved</param>
    /// <param name="request"></param>
    /// <returns>A BorrowDto of the approved borrow request</returns>
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPut("staffs/{borrowId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<BorrowDto>>> ApproveOrRejectRequest(
        [FromRoute] Guid borrowId,
        [FromBody] ApproveOrRejectBorrowRequestRequest request)
    {
        var performingUserId = _currentUserService.GetId();
        var command = new ApproveOrRejectBorrowRequest.Command()
        {
            CurrentUserId = performingUserId,
            BorrowId = borrowId,
            Reason = request.Reason,
            Decision = request.Decision
        };
        var result = await Mediator.Send(command);
        return Ok(Result<BorrowDto>.Succeed(result));
    }
    
    /// <summary>
    /// Check out a borrow request
    /// </summary>
    /// <param name="borrowId">Id of the borrow request to be checked out</param>
    /// <returns>A BorrowDto of the checked out borrow request</returns>
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPost("checkout/{borrowId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<BorrowDto>>> Checkout(
        [FromRoute] Guid borrowId)
    {
        var currentStaff = _currentUserService.GetCurrentUser();
        var command = new CheckoutDocument.Command()
        {
            CurrentStaff = currentStaff,
            BorrowId = borrowId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<BorrowDto>.Succeed(result));
    }
    
    /// <summary>
    /// Return document
    /// </summary>
    /// <param name="documentId">Id of the document to be returned</param>
    /// <returns>A BorrowDto of the returned document borrow request</returns>
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPost("return/{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<BorrowDto>>> Return(
        [FromRoute] Guid documentId)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var command = new ReturnDocument.Command()
        {
            CurrentUser = currentUser,
            DocumentId = documentId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<BorrowDto>.Succeed(result));
    }

    /// <summary>
    /// Update borrow request
    /// </summary>
    /// <param name="borrowId">Id of the borrow request to be updated</param>
    /// <param name="request">Update borrow details</param>
    /// <returns>A BorrowDto of the updated borrow request</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpPut("{borrowId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<BorrowDto>>> Update(
        [FromRoute] Guid borrowId,
        [FromBody] UpdateBorrowRequest request)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        var command = new UpdateBorrow.Command()
        {
            CurrentUser = currentUser,
            BorrowId = borrowId,
            BorrowFrom = request.BorrowFrom,
            BorrowTo = request.BorrowTo,
            Reason = request.Reason,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<BorrowDto>.Succeed(result));
    }
    
    /// <summary>
    /// Cancel borrow request
    /// </summary>
    /// <param name="borrowId">Id of the borrow request to be cancelled</param>
    /// <returns>A BorrowDto of the cancelled borrow request</returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpPost("cancel/{borrowId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<BorrowDto>>> Cancel(
        [FromRoute] Guid borrowId)
    {
        var currentUserId = _currentUserService.GetId();
        var command = new CancelBorrowRequest.Command()
        {
            CurrentUserId = currentUserId,
            BorrowId = borrowId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<BorrowDto>.Succeed(result));
    }
    
    /// <summary>
    /// Get all logs related to requests.
    /// </summary>
    /// <param name="queryParameters">Query parameters</param>
    /// <returns>A list of RequestLogsDtos.</returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpGet("logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<PaginatedList<RequestLogDto>>>> GetAllRequestLogs(
        [FromQuery] GetAllLogsPaginatedQueryParameters queryParameters)
    {
        var query = new GetAllRequestLogsPaginated.Query()
        {
            SearchTerm = queryParameters.SearchTerm,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
        };
        var result = await Mediator.Send(query);
        return Ok(Result<PaginatedList<RequestLogDto>>.Succeed(result));    
    }
}