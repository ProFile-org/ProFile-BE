namespace Api.Controllers.Payload.Requests.Staffs;

/// <summary>
/// Query parameters for getting all staffs with pagination
/// </summary>
public class GetAllStaffsPaginatedQueryParameters
{
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