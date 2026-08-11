using AccountsModule.Contracts.Requests;
using AccountsModule.Contracts.Responses;
using AccountsModule.Messaging;
using AccountsModule.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UserModule.Contracts.Events;

namespace ModularMonolith.Template.AccountsModule.Tests.Messaging;

[Component(TestComponent.AccountsModule)]
[Category(TestCategory.Messaging)]
public sealed class UserCreatedHandlerTests
{
    #region Test Method Declarations

    [Fact]
    public async Task HandleAsync_CreatesAccountFromEvent()
    {
        Mock<IAccountService> accountService = new();
        accountService
            .Setup(service => service.CreateAsync(It.IsAny<CreateAccountRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreateAccountRequest request, CancellationToken _) => new AccountResponse
            {
                Id = 1,
                UserId = request.UserId,
                AccountHolderName = request.AccountHolderName,
                AccountNumber = "ACC-1",
                Balance = request.InitialDeposit
            });

        UserCreatedHandler handler = new(accountService.Object, NullLogger<UserCreatedHandler>.Instance);

        UserCreatedIntegrationEvent message = new()
        {
            Id = 42,
            FullName = "Ada Lovelace",
            Email = "ada@example.com"
        };

        await handler.HandleAsync(message, TestContext.Current.CancellationToken);

        accountService.Verify(
            service => service.CreateAsync(
                It.Is<CreateAccountRequest>(request =>
                    request.UserId == 42 &&
                    request.AccountHolderName == "Ada Lovelace" &&
                    request.InitialDeposit == 0m),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_Throws()
    {
        UserCreatedHandler handler = new(Mock.Of<IAccountService>(), NullLogger<UserCreatedHandler>.Instance);

        Func<Task> act = async () => await handler.HandleAsync(null!, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion
}
