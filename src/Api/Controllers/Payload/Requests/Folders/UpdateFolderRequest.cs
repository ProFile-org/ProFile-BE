namespace Api.Controllers.Payload.Requests.Folders;

public class UpdateFolderRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int Capacity { get; set; }
}