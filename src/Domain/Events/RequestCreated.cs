using Domain.Common;

namespace Domain.Events;

public class RequestCreated : BaseEvent
{
    public RequestCreated(string userName, string requestType, string operation, string documentTitle, Guid documentId, string reason)
    {
        UserName = userName;
        RequestType = requestType;
        Operation = operation;
        DocumentTitle = documentTitle;
        DocumentId = documentId;
        Reason = reason;
    }
    public string UserName { get; }
    public string RequestType { get; }
    public string Operation { get; }
    public string DocumentTitle { get; }
    public string Reason { get; }
    public Guid DocumentId { get; }
}