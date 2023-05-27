namespace Api.Controllers.Payload.Requests.Rooms;

public class GetAllRoomsPaginatedQueryParameters
{
    public int? Page { get; set; }
    public int? Size { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}