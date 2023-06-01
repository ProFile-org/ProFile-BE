namespace Api.Controllers.Payload.Requests.Folders;

/// <summary>
/// Query parameters for getting all folders with pagination
/// </summary>
public class GetAllFoldersPaginatedQueryParameters
{
    /// <summary>
    /// Id of the room to find folders in
    /// </summary>
    public Guid? RoomId { get; set; }
    /// <summary>
    /// Id of the locker to find folders in
    /// </summary>
    public Guid? LockerId { get; set; }
    /// <summary>
    /// Search term
    /// </summary>
    public string? SearchTerm { get; init; }
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