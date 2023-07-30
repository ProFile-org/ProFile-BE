using Application.Common.Messages;
using Microsoft.Extensions.Logging;
using EventId = Application.Common.Logging.EventId;

namespace Application.Common.Extensions.Logging;

public static partial class LockerLogExtension {
    
    [LoggerMessage(Level = LogLevel.Information, Message = LockerLogMessage.Add, EventId = EventId.Add)]
    public static partial void LogAddLocker(this ILogger logger, string lockerId, string roomId, string departmentName);
    
    [LoggerMessage(Level = LogLevel.Information, Message = LockerLogMessage.Remove, EventId = EventId.Remove)]
    public static partial void LogRemoveLocker(this ILogger logger, string lockerId, string roomId, string departmentName);
    
    [LoggerMessage(Level = LogLevel.Information, Message = LockerLogMessage.Update, EventId = EventId.Update)]
    public static partial void LogUpdateLocker(this ILogger logger, string lockerId, string roomId, string departmentName);
}