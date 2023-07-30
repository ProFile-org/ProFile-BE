namespace Api.Controllers.Payload.Requests.Rooms;

/// <summary>
/// Request details to update a room
/// </summary>
public class UpdateRoomRequest
{
    /// <summary>
    /// New name of the room to be updated
    /// </summary>
    public string Name { get; set; } = null!;
    /// <summary>
    /// New description of the room to be updated
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// New capacity of the room to be updated
    /// </summary>
    public int Capacity { get; set; }
    /// <summary>
    /// Room availability
    /// </summary>
    public bool IsAvailable { get; set; }
}