namespace Api.Controllers.Payload.Requests.Folders;

public class GetAllFoldersPaginatedQueryParameters
{
    public Guid? RoomId { get; set; }
    public Guid? LockerId { get; set; }
    public int? Page { get; set; }
    public int? Size { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}