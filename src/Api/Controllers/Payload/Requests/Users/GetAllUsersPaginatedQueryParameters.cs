namespace Api.Controllers.Payload.Requests.Users;

/// <summary>
/// Query parameters for getting all users with pagination
/// </summary>
public class GetAllUsersPaginatedQueryParameters
{
    /// <summary>
    /// Id of the department to find users in
    /// </summary>
    public Guid? DepartmentId { get; set; }
    /// <summary>
    /// Search term
    /// </summary>
    public string? SearchTerm { get; set; }
    /// <summary>
    /// Page number
    /// </summary>
    public int? Page { get; set; }
    /// <summary>
    /// Size number
    /// </summary>
    public int? Size { get; set; }
    /// <summary>
    /// Sort criteria
    /// </summary>
    public string? SortBy { get; set; }
    /// <summary>
    /// Sort direction
    /// </summary>
    public string? SortOrder { get; set; }
}