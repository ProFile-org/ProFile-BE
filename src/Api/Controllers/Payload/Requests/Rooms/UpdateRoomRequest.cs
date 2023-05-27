namespace Api.Controllers.Payload.Requests.Rooms;

public class UpdateRoomRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int Capacity { get; set; }
}