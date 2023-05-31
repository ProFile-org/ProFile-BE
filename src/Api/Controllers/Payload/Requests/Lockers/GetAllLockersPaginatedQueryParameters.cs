using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Payload.Requests.Lockers;

/// <summary>
/// Query parameters for getting all lockers with pagination
/// </summary>
public class GetAllLockersPaginatedQueryParameters
{
    /// <summary>
    /// Id of the room to find lockers in
    /// </summary>
    public Guid? RoomId { get; set; }
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