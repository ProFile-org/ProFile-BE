namespace Api.Controllers.Payload.Requests.Staffs;

/// <summary>
/// Query parameters for getting all staffs with pagination
/// </summary>
public class GetAllStaffsPaginatedQueryParameters : PaginatedQueryParameters
{
    /// <summary>
    /// Search term
    /// </summary>
    public string? SearchTerm { get; set; }
}