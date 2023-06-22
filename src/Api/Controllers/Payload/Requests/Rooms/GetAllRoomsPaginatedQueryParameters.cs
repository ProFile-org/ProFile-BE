namespace Api.Controllers.Payload.Requests.Rooms;

/// <summary>
/// Query parameters for getting all rooms with pagination
/// </summary>
public class GetAllRoomsPaginatedQueryParameters : PaginatedQueryParameters
{
    /// <summary>
    /// Search term
    /// </summary>
    public string? SearchTerm { get; set; }
    public Guid? DepartmentId { get; set; }
}