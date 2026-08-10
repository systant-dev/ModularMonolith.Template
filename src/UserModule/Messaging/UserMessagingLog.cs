using Microsoft.Extensions.Logging;

namespace UserModule.Messaging;

internal static partial class UserMessagingLog
{
    #region Log Message Declarations

    [LoggerMessage(LogLevel.Information, "Account {AccountNumber} opened for user {UserId}")]
    public static partial void AccountOpenedForUser(ILogger logger, int userId, string accountNumber);

    #endregion
}
