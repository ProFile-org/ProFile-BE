namespace Api.Controllers.Payload.Requests.Lockers;

public class UpdateLockerRequest
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int Capacity { get; set; }
}