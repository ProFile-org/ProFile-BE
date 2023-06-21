namespace Api.Controllers.Payload.Requests.Users;

/// <summary>
/// Query parameters for getting all users with pagination
/// </summary>
public class GetAllUsersPaginatedQueryParameters : PaginatedQueryParameters
{
    /// <summary>
    /// Id of the department to find users in
    /// </summary>
    public Guid[]? DepartmentIds { get; set; }
    public string? Role { get; set; }
    /// <summary>
    /// Search term
    /// </summary>
    public string? SearchTerm { get; set; }
}