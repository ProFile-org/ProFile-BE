namespace Api.Controllers.Payload.Requests.Documents;

public class UpdateDocumentRequest
{
    public string Title { get; set; } = null!; 
    public string? Description { get; set; }
    public string DocumentType { get; set; } = null!; 
}