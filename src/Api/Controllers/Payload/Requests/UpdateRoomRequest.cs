namespace Api.Controllers.Payload.Requests;

public class UpdateRoomRequest
{
    public string Name { get; init; }
    public string Description { get; init; }
    public int Capacity { get; init; }
}