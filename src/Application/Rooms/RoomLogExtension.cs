using Application.Common.Messages;
using Microsoft.Extensions.Logging;
using EventId = Application.Common.Logging.EventId;

namespace Application.Rooms;

public static partial class RoomLogExtension {
    [LoggerMessage(Level = LogLevel.Information, Message = RoomLogMessage.Add, EventId = EventId.Add)]
    public static partial void LogAddRoom(this ILogger logger, string roomId, string departmentName);
    
    [LoggerMessage(Level = LogLevel.Information, Message = RoomLogMessage.Remove, EventId = EventId.Remove)]
    public static partial void LogRemoveRoom(this ILogger logger, string roomId, string departmentName);
    
    [LoggerMessage(Level = LogLevel.Information, Message = RoomLogMessage.Update, EventId = EventId.Update)]
    public static partial void LogUpdateRoom(this ILogger logger, string roomId, string departmentName);
}