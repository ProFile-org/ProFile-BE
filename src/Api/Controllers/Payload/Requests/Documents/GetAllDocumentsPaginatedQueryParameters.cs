namespace Api.Controllers.Payload.Requests.Documents;

/// <summary>
/// Query parameters for getting all documents with pagination
/// </summary>
public class GetAllDocumentsPaginatedQueryParameters
{
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
    /// <summary>
    /// Page number
    /// </summary>
    public int? Page { get; set; }
    /// <summary>
    /// Size number
    /// </summary>
    public int? Size { get; set; }
    /// <summary>
    /// Sort criteria
    /// </summary>
    public string? SortBy { get; set; }
    /// <summary>
    /// Sort direction
    /// </summary>
    public string? SortOrder { get; set; }
}