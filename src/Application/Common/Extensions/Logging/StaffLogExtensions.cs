using Application.Common.Messages;
using Microsoft.Extensions.Logging;
using EventId = Application.Common.Logging.EventId;

namespace Application.Common.Extensions.Logging;

public static partial class StaffLogExtensions {
    [LoggerMessage(Level = LogLevel.Information, Message = UserLogMessages.Staff.AssignStaff, EventId = EventId.Add)]
    public static partial void LogAssignStaff(this ILogger logger, string staffId, string roomId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = UserLogMessages.Staff.RemoveFromRoom, EventId = EventId.Remove)]
    public static partial void LogRemoveStaffFromRoom(this ILogger logger, string staffId, string roomId);
}