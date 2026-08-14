using Microsoft.Extensions.Logging;
using UserModule.Models;

namespace UserModule;

internal static partial class UserLog
{
    #region Log Message Declarations

    [LoggerMessage(LogLevel.Information, "User created")]
    public static partial void UserCreated(ILogger logger, [LogProperties] User user);

    [LoggerMessage(LogLevel.Information, "User updated")]
    public static partial void UserUpdated(ILogger logger, [LogProperties] User user);

    #endregion
}
