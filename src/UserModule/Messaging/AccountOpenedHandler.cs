using Microsoft.Extensions.Logging;
using ModularMonolith.Template.SharedKernel.Messaging;
using AccountsModule.Contracts.Events;

namespace UserModule.Messaging;

internal sealed class AccountOpenedHandler(
    ILogger<AccountOpenedHandler> logger) : IMessageHandler<AccountOpenedIntegrationEvent>
{
    #region Public Method Declarations

    public Task HandleAsync(AccountOpenedIntegrationEvent message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        // The Accounts module opened a savings account for one of our users. In a real application we
        // might flag the user record; here we simply acknowledge the cross-module notification.
        UserMessagingLog.AccountOpenedForUser(logger, message.UserId, message.AccountNumber);

        return Task.CompletedTask;
    }

    #endregion
}
