namespace Api.Controllers.Payload.Requests.Lockers;

/// <summary>
/// Request details to update a locker
/// </summary>
public class UpdateLockerRequest
{
    /// <summary>
    /// New name of the locker to be updated
    /// </summary>
    public string Name { get; set; } = null!;
    /// <summary>
    /// New description of the locker to be updated
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// New capacity of the locker to be updated
    /// </summary>
    public int Capacity { get; set; }
    /// <summary>
    /// Status of locker to be updated
    /// </summary>
    public bool IsAvailable { get; set; }
}