using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Payload.Requests.Lockers;

/// <summary>
/// Query parameters for getting all lockers with pagination
/// </summary>
public class GetAllLockersPaginatedQueryParameters : PaginatedQueryParameters
{
    /// <summary>
    /// Id of the room to find lockers in
    /// </summary>
    public Guid? RoomId { get; set; }
}