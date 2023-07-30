namespace Api.Controllers.Payload.Requests.Borrows;

/// <summary>
/// Query parameters for getting all borrow requests with pagination as staff
/// </summary>
public class GetAllBorrowRequestsPaginatedAsStaffQueryParameters : PaginatedQueryParameters
{
    public Guid? DocumentId { get; set; }
    public Guid? EmployeeId { get; set; }
}