namespace Api.Controllers.Payload.Requests.Documents;

/// <summary>
/// Query parameters for getting all documents with pagination
/// </summary>
public class GetAllDocumentsPaginatedQueryParameters : PaginatedQueryParameters
{
    public Guid? UserId { get; set; }
    /// <summary>
    /// Id of the room to find documents in
    /// </summary>
    public Guid? RoomId { get; set; }
    /// <summary>
    /// Id of the locker to find documents in
    /// </summary>
    public Guid? LockerId { get; set; }
    /// <summary>
    /// Id of the folder to find documents in
    /// </summary>
    public Guid? FolderId { get; set; }
    /// <summary>
    /// Search term
    /// </summary>
    public string? SearchTerm { get; set; }
    public string? DocumentStatus { get; set; }
    public bool? IsPrivate { get; set; }
}