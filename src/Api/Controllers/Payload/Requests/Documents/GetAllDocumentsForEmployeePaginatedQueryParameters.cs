namespace Api.Controllers.Payload.Requests.Documents;

public class GetAllDocumentsForEmployeePaginatedQueryParameters : PaginatedQueryParameters
{
    public string? SearchTerm { get; set; }
    public string? DocumentStatus { get; set; }
    public bool IsPrivate { get; set; }
}