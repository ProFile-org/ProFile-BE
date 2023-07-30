using Application.Common.Messages;
using Microsoft.Extensions.Logging;
using EventId = Application.Common.Logging.EventId;

namespace Application.Common.Extensions.Logging;

public static partial class ImportRequestLogExtensions {
    // Approve or reject document
    [LoggerMessage(Level = LogLevel.Information, Message = RequestLogMessages.ApproveImport, EventId = EventId.Approve)]
    public static partial void LogApproveImportRequest(this ILogger logger, string requestId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = RequestLogMessages.RejectImport, EventId = EventId.Reject)]
    public static partial void LogRejectImportRequest(this ILogger logger, string requestId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = DocumentLogMessages.Import.Approve, EventId = EventId.Approve)]
    public static partial void LogApproveImportRequestForDocument(this ILogger logger, string documentId);

    [LoggerMessage(Level = LogLevel.Information, Message = DocumentLogMessages.Import.Reject, EventId = EventId.Reject)]
    public static partial void LogRejectImportRequestForDocument(this ILogger logger, string documentId);

    // Assign document
    [LoggerMessage(Level = LogLevel.Information, Message = DocumentLogMessages.Import.Assign, EventId = EventId.Update)]
    public static partial void LogAssignDocument(this ILogger logger, string documentId, string folderId);

    [LoggerMessage(Level = LogLevel.Information, Message = FolderLogMessage.AssignDocument, EventId = EventId.Update)]
    public static partial void LogAssignDocumentToFolder(this ILogger logger, string documentId);

    // Check in document
    [LoggerMessage(Level = LogLevel.Information, Message = DocumentLogMessages.Import.Checkin, EventId = EventId.Update)]
    public static partial void LogCheckinDocument(this ILogger logger, string documentId);

    [LoggerMessage(Level = LogLevel.Information, Message = RequestLogMessages.CheckInImport, EventId = EventId.Update)]
    public static partial void LogCheckinImportRequest(this ILogger logger, string requestId);
    
    // Request import document
    [LoggerMessage(Level = LogLevel.Information, Message = DocumentLogMessages.Import.NewImportRequest, EventId = EventId.Update)]
    public static partial void LogAddDocument(this ILogger logger, string requestId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = RequestLogMessages.AddImportRequest, EventId = EventId.Update)]
    public static partial void LogAddDocumentRequest(this ILogger logger, string requestId);
}