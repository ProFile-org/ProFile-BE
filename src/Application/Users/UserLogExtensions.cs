using Application.Common.Messages;
using Microsoft.Extensions.Logging;

namespace Application.Users;

public static partial class UserLogExtensions
{
    [LoggerMessage(Level = LogLevel.Information, Message = UserLogMessages.Add)]
    public static partial void LogAddUser(this ILogger logger, string username, string email, string role);
    
    [LoggerMessage(Level = LogLevel.Information, Message = UserLogMessages.Update)]
    public static partial void LogUpdateUser(this ILogger logger, string username);
}