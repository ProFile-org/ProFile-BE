namespace Api.Controllers.Payload.Requests.Rooms;

/// <summary>
/// Query parameters for getting all rooms with pagination
/// </summary>
public class GetAllRoomsPaginatedQueryParameters
{
    /// <summary>
    /// Page number
    /// </summary>
    public int? Page { get; set; }
    /// <summary>
    /// Size number
    /// </summary>
    public int? Size { get; set; }
    /// <summary>
    /// Sort criteria
    /// </summary>
    public string? SortBy { get; set; }
    /// <summary>
    /// Sort direction
    /// </summary>
    public string? SortOrder { get; set; }
}