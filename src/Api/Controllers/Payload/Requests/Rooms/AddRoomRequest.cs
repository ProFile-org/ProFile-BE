namespace Api.Controllers.Payload.Requests.Rooms;

public class AddRoomRequest
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public int Capacity { get; init; }
}