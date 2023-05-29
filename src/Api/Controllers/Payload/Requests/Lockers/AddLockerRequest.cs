namespace Api.Controllers.Payload.Requests.Lockers;

/// <summary>
/// Request details to add a locker
/// </summary>
public class AddLockerRequest
{
    /// <summary>
    /// Name of the locker to be updated
    /// </summary>
    public string Name { get; init; } = null!;
    /// <summary>
    /// Description of the locker to be updated
    /// </summary>
    public string? Description { get; init; }
    /// <summary>
    /// Id of the room that this locker will be in
    /// </summary>
    public Guid RoomId { get; init; }
    /// <summary>
    /// Number of folders this locker can hold
    /// </summary>
    public int Capacity { get; init; }
}