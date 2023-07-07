namespace Api.Controllers.Payload.Requests.Users;

public class GetAllSharedUsersFromASharedEntryPaginatedQueryParameters : PaginatedQueryParameters
{
    public string? SearchTerm { get; set; }
}