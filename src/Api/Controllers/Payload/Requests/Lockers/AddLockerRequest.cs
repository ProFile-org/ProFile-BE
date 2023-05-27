namespace Api.Controllers.Payload.Requests.Lockers;

public class AddLockerRequest
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public Guid RoomId { get; init; }
    public int Capacity { get; init; }
}