namespace Api.Controllers.Payload.Requests.Folders;

public class UpdateFolderRequest
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int Capacity { get; set; }
}