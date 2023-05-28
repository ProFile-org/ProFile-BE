namespace Api.Controllers.Payload.Requests.Staffs;

/// <summary>
/// Request details to add a staff
/// </summary>
public class AddStaffRequest
{
    /// <summary>
    /// User id of the new staff
    /// </summary>
    public Guid UserId { get; init; }
    /// <summary>
    /// Id of the room this staff will be in
    /// </summary>
    public Guid? RoomId { get; init; }
}