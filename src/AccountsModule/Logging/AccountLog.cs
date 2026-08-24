using AccountsModule.Models;
using Microsoft.Extensions.Logging;

namespace AccountsModule;

internal static partial class AccountLog
{
    #region Log Message Declarations

    [LoggerMessage(LogLevel.Information, "Account opened")]
    public static partial void AccountOpened(ILogger logger, [LogProperties] Account account);

    [LoggerMessage(LogLevel.Information, "Account updated")]
    public static partial void AccountUpdated(ILogger logger, [LogProperties] Account account);

    [LoggerMessage(LogLevel.Warning, "Generated account number collided with an existing account; regenerating (attempt {Attempt})")]
    public static partial void AccountNumberCollision(ILogger logger, int attempt);

    [LoggerMessage(LogLevel.Error, "Unable to allocate a unique account number after {Attempts} attempts")]
    public static partial void AccountNumberAllocationFailed(ILogger logger, int attempts);

    #endregion
}
