namespace Api.Controllers.Payload.Requests.Entries;

public class ShareEntryPermissionRequest
{
    public Guid UserId { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool CanView { get; set; }
    public bool CanEdit { get; set; }
}