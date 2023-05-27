using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Payload.Requests.Lockers;


public class GetAllLockersPaginatedQueryParameters
{
    public Guid? RoomId { get; set; }
    public int? Page { get; set; }
    public int? Size { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}