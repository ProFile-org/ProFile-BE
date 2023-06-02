namespace Api.Controllers.Payload.Requests.Borrows;

/// <summary>
/// Query parameters for getting all borrow requests with pagination as staff
/// </summary>
public class GetAllBorrowRequestsPaginatedAsStaffQueryParameters : PaginatedQueryParameters
{
    /// <summary>
    /// Id of the locker to retrieve borrow requests in
    /// </summary>
    public Guid? LockerId { get; set; }
    /// <summary>
    /// Id of the folder to retrieve borrow requests in
    /// </summary>
    public Guid? FolderId { get; set; }
}