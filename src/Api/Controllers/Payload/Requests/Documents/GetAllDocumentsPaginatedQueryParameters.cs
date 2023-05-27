namespace Api.Controllers.Payload.Requests.Documents;

public class GetAllDocumentsPaginatedQueryParameters
{
    public Guid? RoomId { get; set; }
    public Guid? LockerId { get; set; }
    public Guid? FolderId { get; set; }
    public string? SearchTerm { get; set; }
    public int? Page { get; set; }
    public int? Size { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}