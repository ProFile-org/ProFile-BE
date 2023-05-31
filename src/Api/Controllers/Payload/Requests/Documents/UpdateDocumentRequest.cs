namespace Api.Controllers.Payload.Requests.Documents;

/// <summary>
/// Request details to update a document
/// </summary>
public class UpdateDocumentRequest
{
    /// <summary>
    /// New title of the document to be updated
    /// </summary>
    public string Title { get; set; } = null!; 
    /// <summary>
    /// New description of the document to be updated
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// New document type of the document to be updated
    /// </summary>
    public string DocumentType { get; set; } = null!; 
}