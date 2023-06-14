namespace Api.Controllers.Payload.Requests.Documents;

public class SharePermissionsRequest
{
    public Guid[] UserIds { get; set; }
    public bool CanRead { get; set; }
    public bool CanBorrow { get; set; }
    public DateTime ExpiryDate { get; set; }
}