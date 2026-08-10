using Microsoft.Extensions.Logging;
using ModularMonolith.Template.SharedKernel.Messaging;
using AccountsModule.Contracts.Events;
using AccountsModule.Contracts.Requests;
using AccountsModule.Services;
using UserModule.Contracts.Events;

namespace AccountsModule.Messaging;

internal sealed class UserCreatedHandler(
    IAccountService accountService,
    ILogger<UserCreatedHandler> logger) : IMessageHandler<UserCreatedIntegrationEvent>
{
    #region Public Method Declarations

    public async Task HandleAsync(UserCreatedIntegrationEvent message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        // A new user was created in the User module, so automatically open a savings account for them.
        await accountService.CreateAsync(
            new CreateAccountRequest
            {
                UserId = message.Id,
                AccountHolderName = message.FullName,
                InitialDeposit = 0
            },
            cancellationToken).ConfigureAwait(false);

        AccountMessagingLog.SavingsAccountOpenedForUser(logger, message.Id);
    }

    #endregion
}
