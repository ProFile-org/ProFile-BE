namespace Api.Controllers.Payload.Requests;

/// <summary>
///  get all logs paginated
/// </summary>
public class GetAllLogsPaginatedQueryParameters : PaginatedQueryParameters
{
    public string? SearchTerm { get; set; }    
}