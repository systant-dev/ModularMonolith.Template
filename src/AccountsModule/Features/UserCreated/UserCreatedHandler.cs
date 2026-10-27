using AccountsModule.Contracts.Requests;
using AccountsModule.Contracts.Responses;
using AccountsModule.Features.Accounts;
using Microsoft.Extensions.Logging;
using ModularMonolith.Template.SharedKernel.Messaging.Handling;
using ModularMonolith.Template.SharedKernel.Results;
using UserModule.Contracts.Events;

namespace AccountsModule.Features.UserCreated;

internal sealed class UserCreatedHandler(IAccountService accountService, ILogger<UserCreatedHandler> logger)
    : IMessageHandler<UserCreatedIntegrationEvent>
{
    #region Public Method Declarations

    public async Task HandleAsync(UserCreatedIntegrationEvent message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        CreateAccountRequest request = new()
        {
            UserId = message.Id,
            AccountHolderName = message.FullName,
            InitialDeposit = 0
        };

        Result<AccountResponse> result = await accountService.CreateAsync(request, cancellationToken);
        if (result.IsSuccess)
        {
            AccountMessagingLog.SavingsAccountOpenedForUser(logger, message.Id);
        }
        else
        {
            AccountMessagingLog.SavingsAccountNotOpenedForUser(logger, message.Id, result.Error.Message);
        }
    }

    #endregion
}
