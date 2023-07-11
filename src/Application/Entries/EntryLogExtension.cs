using Application.Common.Messages;
using Microsoft.Extensions.Logging;
using EventId = Application.Common.Logging.EventId;

namespace Application.Entries;

public static partial class EntryLogExtension
{
    [LoggerMessage(Level = LogLevel.Information, Message = EntryLogMessages.DownloadDigitalFile, EventId = Common.Logging.EventId.Other)]
    public static partial void LogDownLoadFile(this ILogger logger, string username, string fileId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = EntryLogMessages.MoveEntryToBin, EventId = Common.Logging.EventId.Update)]
    public static partial void LogMoveEntryToBin(this ILogger logger, string username, string entryId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = EntryLogMessages.ShareEntry, EventId = EventId.OtherRequestRelated)]
    public static partial void LogShareEntry(this ILogger logger, string username, string entryId, string sharedUsername);
    
    [LoggerMessage(Level = LogLevel.Information, Message = EntryLogMessages.UpdateEntry, EventId = EventId.Update)]
    public static partial void LogUpdateEntry(this ILogger logger, string username, string entryId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = EntryLogMessages.UploadDigitalFile, EventId = EventId.Add)]
    public static partial void LogCreateEntry(this ILogger logger, string username, string entryId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = EntryLogMessages.UploadSharedEntry, EventId = EventId.Add)]
    public static partial void LogCreateSharedEntry(this ILogger logger, string username, string entryId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = EntryLogMessages.DeleteBinEntry, EventId = EventId.Remove)]
    public static partial void LogDeleteBinEntry(this ILogger logger, string username, string entryId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = EntryLogMessages.RestoreBinEntry, EventId = EventId.Update)]
    public static partial void LogRestoreBinEntry(this ILogger logger, string username, string entryId);
}

