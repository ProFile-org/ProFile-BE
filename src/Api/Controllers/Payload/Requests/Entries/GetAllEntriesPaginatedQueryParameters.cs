namespace Api.Controllers.Payload.Requests.Entries;

/// <summary>
/// Get All Entries Paginated Query Parameters
/// </summary>
public class GetAllEntriesPaginatedQueryParameters : PaginatedQueryParameters
{
    /// <summary>
    /// Entry id
    /// </summary>
    public Guid? EntryId { get; set; }
}