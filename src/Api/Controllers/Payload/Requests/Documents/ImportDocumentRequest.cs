namespace Api.Controllers.Payload.Requests.Documents;

/// <summary>
/// Request details to import a document
/// </summary>
public class ImportDocumentRequest
{
    /// <summary>
    /// Title of the document to be imported
    /// </summary>
    public string Title { get; set; } = null!;
    /// <summary>
    /// Description of the document to be imported
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// Document type of the document to be imported
    /// </summary>
    public string DocumentType { get; set; } = null!;
    /// <summary>
    /// Id of the importer
    /// </summary>
    public Guid ImporterId { get; set; }
    /// <summary>
    /// Id of the folder that this document will be in
    /// </summary>
    public Guid FolderId { get; set; }
}