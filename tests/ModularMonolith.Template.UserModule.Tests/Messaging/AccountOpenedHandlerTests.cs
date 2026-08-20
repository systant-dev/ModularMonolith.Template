using AccountsModule.Contracts.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using UserModule.Messaging;

namespace ModularMonolith.Template.UserModule.Tests.Messaging;

[Component(TestComponent.UserModule)]
[Category(TestCategory.Messaging)]
public sealed class AccountOpenedHandlerTests
{
    #region Test Method Declarations

    [Fact]
    public async Task HandleAsync_WithValidMessage_CompletesSuccessfully()
    {
        AccountOpenedHandler handler = new(NullLogger<AccountOpenedHandler>.Instance);

        AccountOpenedIntegrationEvent message = new()
        {
            AccountId = 7,
            UserId = 42,
            AccountNumber = "ACC-1"
        };

        Func<Task> act = async () => await handler.HandleAsync(message, TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_Throws()
    {
        AccountOpenedHandler handler = new(NullLogger<AccountOpenedHandler>.Instance);

        Func<Task> act = async () => await handler.HandleAsync(null!, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion
}
