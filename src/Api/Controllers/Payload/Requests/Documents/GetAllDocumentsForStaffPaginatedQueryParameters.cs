namespace Api.Controllers.Payload.Requests.Documents;

public class GetAllDocumentsForStaffPaginatedQueryParameters : PaginatedQueryParameters
{
    /// <summary>
    /// Id of the locker to find documents in
    /// </summary>
    public Guid? LockerId { get; set; }
    /// <summary>
    /// Id of the folder to find documents in
    /// </summary>
    public Guid? FolderId { get; set; }
    /// <summary>
    /// Search term
    /// </summary>
    public string? SearchTerm { get; set; }
}