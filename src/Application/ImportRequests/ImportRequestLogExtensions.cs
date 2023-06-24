using Application.Common.Messages;
using Microsoft.Extensions.Logging;
using EventId = Application.Common.Logging.EventId;

namespace Application.ImportRequests;

public static partial class ImportRequestLogExtensions {
    [LoggerMessage(Level = LogLevel.Information, Message = RequestLogMessages.ApproveImport, EventId = EventId.Approve)]
    public static partial void LogApproveImportRequest(this ILogger logger, string requestId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = RequestLogMessages.RejectImport, EventId = EventId.Reject)]
    public static partial void LogRejectImportRequest(this ILogger logger, string requestId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = DocumentLogMessages.Import.Approve, EventId = EventId.Approve)]
    public static partial void LogApproveImportRequestForDocument(this ILogger logger, string documentId);

    [LoggerMessage(Level = LogLevel.Information, Message = DocumentLogMessages.Import.Reject, EventId = EventId.Reject)]
    public static partial void LogRejectImportRequestForDocument(this ILogger logger, string documentId);
}