namespace Api.Controllers.Payload.Requests.ImportRequests;

/// <summary>
/// 
/// </summary>
public class GetAllImportRequestsPaginatedQueryParameters : PaginatedQueryParameters
{
    public string? SearchTerm { get; set; }
    public Guid? RoomId { get; set; }
}