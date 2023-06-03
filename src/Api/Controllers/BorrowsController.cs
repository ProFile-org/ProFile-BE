using Api.Controllers.Payload.Requests.Borrows;
using Application.Borrows.Commands;
using Application.Borrows.Queries;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

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
    public async Task<ActionResult<Result<BorrowDto>>> BorrowDocument([FromBody] BorrowDocumentRequest request)
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
    
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpGet("staffs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<PaginatedList<BorrowDto>>>> GetAllRequestsAsStaffPaginated(
        [FromQuery] GetAllBorrowRequestsPaginatedAsStaffQueryParameters queryParameters)
    {
        var departmentId = _currentUserService.GetCurrentDepartmentForStaff();
        var command = new GetAllBorrowRequestsPaginated.Query()
        {
            DepartmentId = departmentId,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<PaginatedList<BorrowDto>>.Succeed(result));
    }
    
    /// <summary>
    /// Get all borrow requests as admin paginated
    /// </summary>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<PaginatedList<BorrowDto>>>> GetAllRequestsAsAdminPaginated(
        [FromQuery] GetAllBorrowRequestsPaginatedAsAdminQueryParameters queryParameters)
    {
        var command = new GetAllBorrowRequestsPaginated.Query()
        {
            DepartmentId = queryParameters.DepartmentId,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<PaginatedList<BorrowDto>>.Succeed(result));
    }

    /// <summary>
    /// Get all borrow requests as admin paginated
    /// </summary>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet("employees")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<PaginatedList<BorrowDto>>>> GetAllRequestsAsEmployeePaginated(
        [FromQuery] GetAllBorrowRequestsPaginatedAsEmployeeQueryParameters queryParameters)
    {
        var userId = _currentUserService.GetCurrentUser().Id;
        var command = new GetAllBorrowRequestsSpecificPaginated.Query()
        {
            EmployeeId = userId,
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
    /// Get all borrow requests for a document paginated
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    [RequiresRole(IdentityData.Roles.Employee)]
    [HttpGet("documents/{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<PaginatedList<BorrowDto>>>> GetAllRequestsForDocumentPaginated(
        [FromRoute] Guid documentId,
        [FromQuery] GetAllBorrowRequestsPaginatedAsEmployeeQueryParameters queryParameters)
    {
        var command = new GetAllBorrowRequestsSpecificPaginated.Query()
        {
            DocumentId = documentId,
            Page = queryParameters.Page,
            Size = queryParameters.Size,
            SortBy = queryParameters.SortBy,
            SortOrder = queryParameters.SortOrder,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<PaginatedList<BorrowDto>>.Succeed(result));
    }
    
    /// <summary>
    /// Approve a borrow request
    /// </summary>
    /// <param name="borrowId">Id of the borrow request to be approved</param>
    /// <returns>A BorrowDto of the approved borrow request</returns>
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPost("approve/{borrowId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<BorrowDto>>> ApproveRequest([FromRoute] Guid borrowId)
    {
        var command = new ApproveBorrowRequest.Command()
        {
            BorrowId = borrowId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<BorrowDto>.Succeed(result));
    }
    
    /// <summary>
    /// Reject a borrow request
    /// </summary>
    /// <param name="borrowId">Id of the borrow request to be rejected</param>
    /// <returns>A BorrowDto of the rejected borrow request</returns>
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpPost("reject/{borrowId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<BorrowDto>>> RejectRequest([FromRoute] Guid borrowId)
    {
        var command = new RejectBorrowRequest.Command()
        {
            BorrowId = borrowId,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<BorrowDto>.Succeed(result));
    }
}