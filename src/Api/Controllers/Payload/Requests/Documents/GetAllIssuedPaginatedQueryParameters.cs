namespace Api.Controllers.Payload.Requests.Documents;

public class GetAllIssuedPaginatedQueryParameters : PaginatedQueryParameters
{
    public string? SearchTerm { get; set; }
}