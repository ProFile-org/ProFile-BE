namespace Api.Controllers.Payload.Requests.Staffs;

public class AddStaffRequest
{
    public Guid UserId { get; init; }
    public Guid? RoomId { get; init; }
}