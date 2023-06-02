namespace Api.Controllers.Payload.Requests.Borrows;

/// <summary>
/// Query parameters for getting all borrow requests with pagination as admin
/// </summary>
public class GetAllBorrowRequestsPaginatedAsAdminQueryParameters : PaginatedQueryParameters
{
    /// <summary>
    /// Id of the room to retrieve borrow requests in
    /// </summary>
    public Guid? RoomId { get; set; }
    /// <summary>
    /// Id of the locker to retrieve borrow requests in
    /// </summary>
    public Guid? LockerId { get; set; }
    /// <summary>
    /// Id of the folder to retrieve borrow requests in
    /// </summary>
    public Guid? FolderId { get; set; }
}