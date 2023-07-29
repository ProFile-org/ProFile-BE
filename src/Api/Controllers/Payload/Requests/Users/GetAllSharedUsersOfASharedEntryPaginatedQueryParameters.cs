namespace Api.Controllers.Payload.Requests.Users;

public class GetAllSharedUsersPaginatedQueryParameters
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
}