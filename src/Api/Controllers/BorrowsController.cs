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
        var command = new BorrowDocument.Command()
        {
            BorrowerId = request.BorrowerId,
            DocumentId = request.DocumentId,
            BorrowTo = request.BorrowTo,
            Reason = request.Reason,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<BorrowDto>.Succeed(result));
    }
    
    [RequiresRole(IdentityData.Roles.Staff)]
    [HttpGet("staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<PaginatedList<BorrowDto>>>> GetAllBorrowRequestsAsStaffPaginated(
        [FromQuery] GetAllBorrowRequestsPaginatedAsStaffQueryParameters asAdminQueryParameters)
    {
        var roomId = _currentUserService.GetCurrentRoomForStaff();
        var command = new GetAllBorrowRequestsPaginated.Query()
        {
            RoomId = roomId,
            LockerId = asAdminQueryParameters.LockerId,
            FolderId = asAdminQueryParameters.FolderId,
            SearchTerm = asAdminQueryParameters.SearchTerm,
            Page = asAdminQueryParameters.Page,
            Size = asAdminQueryParameters.Size,
            SortBy = asAdminQueryParameters.SortBy,
            SortOrder = asAdminQueryParameters.SortOrder,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<PaginatedList<BorrowDto>>.Succeed(result));
    }
    
    [RequiresRole(IdentityData.Roles.Admin)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<PaginatedList<BorrowDto>>>> GetAllBorrowRequestsAsAdminPaginated(
        [FromQuery] GetAllBorrowRequestsPaginatedAsAdminQueryParameters asAdminQueryParameters)
    {
        var command = new GetAllBorrowRequestsPaginated.Query()
        {
            RoomId = asAdminQueryParameters.RoomId,
            LockerId = asAdminQueryParameters.LockerId,
            FolderId = asAdminQueryParameters.FolderId,
            SearchTerm = asAdminQueryParameters.SearchTerm,
            Page = asAdminQueryParameters.Page,
            Size = asAdminQueryParameters.Size,
            SortBy = asAdminQueryParameters.SortBy,
            SortOrder = asAdminQueryParameters.SortOrder,
        };
        var result = await Mediator.Send(command);
        return Ok(Result<PaginatedList<BorrowDto>>.Succeed(result));
    }
}