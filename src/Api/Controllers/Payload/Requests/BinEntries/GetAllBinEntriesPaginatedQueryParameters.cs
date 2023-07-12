namespace Api.Controllers.Payload.Requests.BinEntries;

public class GetAllBinEntriesPaginatedQueryParameters : PaginatedQueryParameters
{
    public string EntryPath { get; set; } = null!;
    public string? SearchTerm { get; set; }
}