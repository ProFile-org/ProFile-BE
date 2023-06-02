namespace Api.Controllers.Payload.Requests.Folders;

/// <summary>
/// Query parameters for getting all folders with pagination
/// </summary>
public class GetAllFoldersPaginatedQueryParameters : PaginatedQueryParameters
{
    /// <summary>
    /// Id of the room to find folders in
    /// </summary>
    public Guid? RoomId { get; set; }
    /// <summary>
    /// Id of the locker to find folders in
    /// </summary>
    public Guid? LockerId { get; set; }
}