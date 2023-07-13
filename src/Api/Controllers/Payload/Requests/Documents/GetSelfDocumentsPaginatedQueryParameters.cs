namespace Api.Controllers.Payload.Requests.Documents;

/// <summary>
/// Query parameters for getting all documents that belong to an employee
/// </summary>
public class GetSelfDocumentsPaginatedQueryParameters : PaginatedQueryParameters
{
    public string? SearchTerm { get; set; }
}