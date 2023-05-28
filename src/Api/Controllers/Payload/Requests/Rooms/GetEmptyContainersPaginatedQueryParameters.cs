namespace Api.Controllers.Payload.Requests.Rooms;

/// <summary>
/// Query parameters for getting all empty containers in a room
/// </summary>
public class GetEmptyContainersPaginatedQueryParameters
{
    /// <summary>
    /// Page number
    /// </summary>
    public int? Page { get; set; }
    /// <summary>
    /// Size number
    /// </summary>
    public int? Size { get; set; }
}