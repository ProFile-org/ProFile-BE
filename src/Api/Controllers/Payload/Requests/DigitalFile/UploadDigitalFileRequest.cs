namespace Api.Controllers.Payload.Requests.DigitalFile;

/// <summary>
/// 
/// </summary>
public class UploadDigitalFileRequest
{
    public string Directory { get; set; } = null!;
    public IFormFile File { get; set; } = null!;
}