namespace Api.Controllers.Payload.Requests.Borrows;

public class GetAllBorrowRequestsPaginatedForDocumentQueryParameters : PaginatedQueryParameters
{
    public string? Status { get; set; }
}