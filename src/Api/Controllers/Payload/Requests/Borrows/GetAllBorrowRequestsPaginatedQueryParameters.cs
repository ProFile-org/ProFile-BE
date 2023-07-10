namespace Api.Controllers.Payload.Requests.Borrows;

/// <summary>
/// Query parameters for getting all borrow requests with pagination as admin
/// </summary>
public class GetAllBorrowRequestsPaginatedQueryParameters : PaginatedQueryParameters
{
    /// <summary>
    /// Id of the room to get borrow requests in
    /// </summary>
    public Guid? RoomId { get; set; }
    public Guid? DocumentId { get; set; }
    public Guid? EmployeeId { get; set; }
    public string[]? Statuses { get; init; }
}