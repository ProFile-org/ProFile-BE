using Application.Common.Messages;
using Microsoft.Extensions.Logging;

namespace Application.Common.Extensions.Logging;

public static partial class BorrowLogExtensions
{
    [LoggerMessage(Level = LogLevel.Information, Message = DocumentLogMessages.Borrow.NewBorrowRequest, EventId = Common.Logging.EventId.Add)]
    public static partial void LogBorrowDocument(this ILogger logger, string documentId, string borrowId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = DocumentLogMessages.Borrow.Cancel, EventId = Common.Logging.EventId.OtherRequestRelated)]
    public static partial void LogCancelBorrowRequest(this ILogger logger, string documentId, string borrowId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = DocumentLogMessages.Borrow.Checkout, EventId = Common.Logging.EventId.OtherRequestRelated)]
    public static partial void LogCheckoutDocument(this ILogger logger, string documentId, string borrowId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = DocumentLogMessages.Borrow.Return, EventId = Common.Logging.EventId.OtherRequestRelated)]
    public static partial void LogReturnDocument(this ILogger logger, string documentId, string borrowId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = DocumentLogMessages.Borrow.Return, EventId = Common.Logging.EventId.Update)]
    public static partial void LogUpdateBorrow(this ILogger logger, string documentId, string borrowId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = DocumentLogMessages.Borrow.Approve, EventId = Common.Logging.EventId.Approve)]
    public static partial void LogApproveBorrowRequest(this ILogger logger, string documentId, string borrowId);
    
    [LoggerMessage(Level = LogLevel.Information, Message = DocumentLogMessages.Borrow.Reject, EventId = Common.Logging.EventId.Reject)]
    public static partial void LogRejectBorrowRequest(this ILogger logger, string documentId, string borrowId);
}