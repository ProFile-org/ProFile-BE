namespace Api.Controllers.Payload.Requests.Borrows;

public class ApproveOrRejectBorrowRequestRequest
{
    public string StaffReason { get; set; }
    public string Decision { get; set; }
}