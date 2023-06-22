namespace Api.Controllers.Payload.Requests.Documents;

public class GetAllDocumentsForEmployeePaginatedQueryParameters : PaginatedQueryParameters
{
    public Guid? UserId { get; set; }
    public string? SearchTerm { get; set; }
    public string? DocumentStatus { get; set; }
    public bool IsPrivate { get; set; }
}