using Application.Common.Messages;
using Microsoft.Extensions.Logging;
using EventId = Application.Common.Logging.EventId;

namespace Application.Documents;

public static partial class DocumentLogExtensions
{
    [LoggerMessage(Level = LogLevel.Information, Message = DocumentLogMessages.Import.NewImport, EventId = EventId.Add)]
    public static partial void LogImportDocument(this ILogger logger, string documentId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = DocumentLogMessages.Delete, EventId = EventId.Remove)]
    public static partial void LogDeleteDocument(this ILogger logger, string documentId);
    

    [LoggerMessage(Level = LogLevel.Information, Message = DocumentLogMessages.Update, EventId = EventId.Update)]
    public static partial void LogUpdateDocument(this ILogger logger, string documentId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = DocumentLogMessages.Grant, EventId = EventId.Update)]
    public static partial void LogGrantPermission(this ILogger logger, string permission, string username);
    
    [LoggerMessage(Level = LogLevel.Information, Message = DocumentLogMessages.Revoke, EventId = EventId.Update)]
    public static partial void LogRevokePermission(this ILogger logger, string permission, string username);
}