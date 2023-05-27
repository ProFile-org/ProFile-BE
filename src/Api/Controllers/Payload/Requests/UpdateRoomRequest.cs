namespace Api.Controllers.Payload.Requests;

public class UpdateRoomRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int Capacity { get; set; }
}