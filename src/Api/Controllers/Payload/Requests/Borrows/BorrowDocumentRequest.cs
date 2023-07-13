namespace Api.Controllers.Payload.Requests.Borrows;

/// <summary>
/// Request details to borrow a document
/// </summary>
public class BorrowDocumentRequest
{
    /// <summary>
    /// Id of the document to be borrowed
    /// </summary>
    public Guid DocumentId { get; set; }
    /// <summary>
    /// Borrow from
    /// </summary>
    public DateTime BorrowFrom { get; set; }
    /// <summary>
    /// Borrow to
    /// </summary>
    public DateTime BorrowTo { get; set; }
    /// <summary>
    /// Reason of borrowing
    /// </summary>
    public string Reason { get; set; } = null!;
}