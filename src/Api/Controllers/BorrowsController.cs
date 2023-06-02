using Api.Controllers.Payload.Requests.Borrows;
using Application.Borrows.Commands;
using Application.Borrows.Queries;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class BorrowsController : ApiControllerBase
{
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
    
    [RequiresRole(IdentityData.Roles.Admin, IdentityData.Roles.Staff)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<PaginatedList<BorrowDto>>>> GetAllBorrowRequestsPaginated(
        [FromQuery] GetAllBorrowRequestsPaginatedQueryParameters queryParameters)
    {
        var command = new GetAllBorrowRequestsPaginated.Query()
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
        var result = await Mediator.Send(command);
        return Ok(Result<PaginatedList<BorrowDto>>.Succeed(result));
    }
}