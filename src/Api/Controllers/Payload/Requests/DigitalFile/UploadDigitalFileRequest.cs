namespace Api.Controllers.Payload.Requests.DigitalFile;

/// <summary>
/// 
/// </summary>
public class UploadDigitalFileRequest {
    public string Path { get; set; }
    public IFormFile File { get; set; }
}