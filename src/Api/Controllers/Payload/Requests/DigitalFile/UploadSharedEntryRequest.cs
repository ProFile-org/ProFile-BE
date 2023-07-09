namespace Api.Controllers.Payload.Requests.DigitalFile;

/// <summary>
/// 
/// </summary>
public class UploadSharedEntryRequest
{
    public string Name { get; set; } = null!;
    public bool IsDirectory { get; set; }
    public IFormFile? File { get; set; }
}