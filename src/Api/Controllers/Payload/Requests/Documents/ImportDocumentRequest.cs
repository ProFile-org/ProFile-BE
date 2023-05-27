namespace Api.Controllers.Payload.Requests.Documents;

public class ImportDocumentRequest
{
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public string DocumentType { get; init; } = null!;
    public Guid ImporterId { get; init; }
    public Guid FolderId { get; init; }
}