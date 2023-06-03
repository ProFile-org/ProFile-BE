namespace Api.Controllers.Payload.Requests.Borrows;

/// <summary>
/// Query parameters for getting all borrow requests with pagination as admin
/// </summary>
public class GetAllBorrowRequestsPaginatedAsAdminQueryParameters : PaginatedQueryParameters
{
    /// <summary>
    /// Id of the department to get borrow requests in
    /// </summary>
    public Guid? DepartmentId { get; set; }
    public Guid? DocumentId { get; set; }
    public Guid? EmployeeId { get; set; }
}