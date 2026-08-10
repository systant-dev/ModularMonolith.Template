using Microsoft.Extensions.Logging;

namespace AccountsModule.Messaging;

internal static partial class AccountMessagingLog
{
    #region Log Message Declarations

    [LoggerMessage(LogLevel.Information, "Savings account opened for user {UserId}")]
    public static partial void SavingsAccountOpenedForUser(ILogger logger, int userId);

    #endregion
}
