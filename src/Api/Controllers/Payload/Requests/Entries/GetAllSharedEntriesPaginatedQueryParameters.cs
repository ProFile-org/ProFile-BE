namespace Api.Controllers.Payload.Requests.Entries;

/// <summary>
/// Get All Shared Entries Paginated Query Parameters
/// </summary>
public class GetAllSharedEntriesPaginatedQueryParameters : PaginatedQueryParameters
{
    /// <summary>
    /// Entry id
    /// </summary>
    public Guid? EntryId { get; set; }
}