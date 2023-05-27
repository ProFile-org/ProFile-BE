namespace Api.Controllers.Payload.Requests.Users;

public class GetAllUsersPaginatedQueryParameters
{
    public Guid? DepartmentId { get; set; }
    public string? SearchTerm { get; set; }
    public int? Page { get; set; }
    public int? Size { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}