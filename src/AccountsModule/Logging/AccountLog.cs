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

    #endregion
}
