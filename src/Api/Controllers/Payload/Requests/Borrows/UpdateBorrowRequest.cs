namespace Api.Controllers.Payload.Requests.Borrows;

public class UpdateBorrowRequest
{
    public DateTime BorrowFrom { get; init; }
    public DateTime BorrowTo { get; init; }
    public string Reason { get; init; } = null!;
}