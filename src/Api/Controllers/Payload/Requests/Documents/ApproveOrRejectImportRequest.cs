namespace Api.Controllers.Payload.Requests.Documents;

public class ApproveOrRejectImportRequest
{
    public string Decision { get; set; } = null!;
    public string Reason { get; set; } = null!;
}