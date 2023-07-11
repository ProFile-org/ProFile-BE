namespace Api.Controllers.Payload.Requests.BinEntries;

public class GetAllBinEntriesPaginatedQueryParameters : PaginatedQueryParameters
{
    public string? SearchTerm { get; set; }
}