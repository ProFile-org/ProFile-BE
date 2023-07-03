namespace Api.Controllers.Payload.Requests.Entries;

/// <summary>
/// Get All Entries Paginated Query Parameters
/// </summary>
public class GetAllEntriesPaginatedQueryParameters : PaginatedQueryParameters
{
    public string EntryPath { get; set; } = null!;
}