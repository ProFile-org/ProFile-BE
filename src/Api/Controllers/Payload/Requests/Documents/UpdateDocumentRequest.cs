namespace Api.Controllers.Payload.Requests.Documents;

public class UpdateDocumentRequest
{
    public string Title { get; set; } = null!; 
    public string Description { get; set; } = null!; 
    public string DocumentType { get; set; } = null!; 
}