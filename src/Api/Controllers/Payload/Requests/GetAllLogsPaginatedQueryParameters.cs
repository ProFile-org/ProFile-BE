namespace Api.Controllers.Payload.Requests;

public class GetAllLogsPaginatedQueryParameters : PaginatedQueryParameters
{
    public string? SearchTerm { get; set; }
}