namespace Api.Controllers.Payload.Requests.Rooms;

/// <summary>
/// Request details to add a room
/// </summary>
public class AddRoomRequest
{
    /// <summary>
    /// Name of the room to be added
    /// </summary>
    public string Name { get; set; } = null!;
    /// <summary>
    /// Description of the room to be added
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// Number of lockers this room can hold
    /// </summary>
    public int Capacity { get; set; }
    /// <summary>
    /// Id of the department this room belongs to
    /// </summary>
    public Guid DepartmentId { get; set; }
}