using AccountsModule.Contracts.Events;
using Microsoft.Extensions.Logging;
using ModularMonolith.Template.SharedKernel.Messaging.Handling;

namespace UserModule.Messaging;

internal sealed class AccountOpenedHandler(ILogger<AccountOpenedHandler> logger) : IMessageHandler<AccountOpenedIntegrationEvent>
{
    #region Public Method Declarations

    public Task HandleAsync(AccountOpenedIntegrationEvent message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        UserMessagingLog.AccountOpenedForUser(logger, message.UserId, message.AccountNumber);
        return Task.CompletedTask;
    }

    #endregion
}
