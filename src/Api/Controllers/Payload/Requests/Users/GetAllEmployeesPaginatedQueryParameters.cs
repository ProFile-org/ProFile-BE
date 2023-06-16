namespace Api.Controllers.Payload.Requests.Users;

public class GetAllEmployeesPaginatedQueryParameters : PaginatedQueryParameters
{
    /// <summary>
    /// Search term
    /// </summary>
    public string? SearchTerm { get; set; }
}