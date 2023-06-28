using Application.Common.Messages;
using Microsoft.Extensions.Logging;
using EventId = Application.Common.Logging.EventId;

namespace Application.Folders;

public static partial class FolderLogExtension {
    
    [LoggerMessage(Level = LogLevel.Information, Message = FolderLogMessage.Add, EventId = EventId.Add)]
    public static partial void LogAddFolder(this ILogger logger, string folderId, string lockerId, string roomId, string departmentName);

    [LoggerMessage(Level = LogLevel.Information, Message = FolderLogMessage.Remove, EventId = EventId.Remove)]
    public static partial void LogRemoveFolder(this ILogger logger, string folderId, string lockerId, string roomId, string departmentName);

    [LoggerMessage(Level = LogLevel.Information, Message = FolderLogMessage.Update, EventId = EventId.Update)]
    public static partial void LogUpdateFolder(this ILogger logger, string folderId, string lockerId, string roomId, string departmentName);
}