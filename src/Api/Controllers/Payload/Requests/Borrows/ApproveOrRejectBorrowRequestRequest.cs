namespace Api.Controllers.Payload.Requests.Borrows;

public class ApproveOrRejectBorrowRequestRequest
{
    public string Reason { get; set; }
    public string Decision { get; set; }
}