namespace Api.Controllers.Payload.Requests.Borrows;

/// <summary>
/// Query parameters for getting all borrow requests with pagination as employee
/// </summary>
public class GetAllBorrowRequestsPaginatedAsEmployeeQueryParameters : PaginatedQueryParameters
{
    public Guid? DocumentId { get; set; }
}