namespace Api.Controllers.Payload.Requests;

public class UpdateRoomRequest
{
    public string Description { get; set; }
    public Guid StaffId { get; set; }
    public int Capacity { get; set; }
}