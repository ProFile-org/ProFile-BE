using Application.Common.Messages;
using Microsoft.Extensions.Logging;
using EventId = Application.Common.Logging.EventId;

namespace Application.Common.Extensions.Logging;

public static partial class LoginLogExtension
{
    [LoggerMessage(Level = LogLevel.Information, Message = LoginLogMessages.Login, EventId = EventId.Approve)]
    public static partial void LogLogin(this ILogger logger, string username, string time);
}