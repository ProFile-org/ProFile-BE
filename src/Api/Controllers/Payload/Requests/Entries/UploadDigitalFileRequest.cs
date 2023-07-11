namespace Api.Controllers.Payload.Requests.Entries;

/// <summary>
/// 
/// </summary>
public class UploadDigitalFileRequest
{
    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
    public bool IsDirectory { get; set; }
    public IFormFile? File { get; set; }
}