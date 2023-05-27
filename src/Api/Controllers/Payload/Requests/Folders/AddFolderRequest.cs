namespace Api.Controllers.Payload.Requests.Folders;

public class AddFolderRequest
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public int Capacity { get; init; }
    public Guid LockerId { get; init; }
}