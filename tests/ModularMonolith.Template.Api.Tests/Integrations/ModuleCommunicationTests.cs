using AccountsModule.Contracts.Events;
using AccountsModule.Contracts.Responses;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.Api.Tests.Infrastructure;
using ModularMonolith.Template.SharedKernel.Messaging;
using System.Net.Http.Json;
using UserModule.Contracts.Events;

namespace ModularMonolith.Template.Api.Tests.Integrations;

public sealed class ModuleCommunicationTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    #region Field Declarations

    private readonly TestWebApplicationFactory factory = factory;

    #endregion

    #region Test Method Declarations

    [Fact]
    public void DispatchMap_CanDispatch_UserCreatedIntegrationEvent()
    {
        IMessageDispatchMap dispatchMap = factory.Services.GetRequiredService<IMessageDispatchMap>();

        dispatchMap.CanDispatch(typeof(UserCreatedIntegrationEvent)).Should().BeTrue();
    }

    [Fact]
    public void DispatchMap_CanDispatch_AccountOpenedIntegrationEvent()
    {
        IMessageDispatchMap dispatchMap = factory.Services.GetRequiredService<IMessageDispatchMap>();

        dispatchMap.CanDispatch(typeof(AccountOpenedIntegrationEvent)).Should().BeTrue();
    }

    [Fact]
    public async Task UserCreatedIntegrationEvent_OpensSavingsAccountForUser()
    {
        UserCreatedIntegrationEvent userCreated = new()
        {
            Id = 4242,
            FullName = "Ada Lovelace",
            Email = "ada@example.com"
        };

        await PublishAsync(userCreated);

        HttpClient client = factory.CreateClient();
        IReadOnlyList<AccountResponse>? accounts = await client.GetFromJsonAsync<IReadOnlyList<AccountResponse>>(
            "/v1/accounts",
            TestContext.Current.CancellationToken);

        accounts.Should().NotBeNull();
        accounts!.Should().ContainSingle(account => account.UserId == userCreated.Id)
            .Which.AccountHolderName.Should().Be(userCreated.FullName);
    }

    [Fact]
    public async Task AccountOpenedIntegrationEvent_IsHandledByUserModule()
    {
        AccountOpenedIntegrationEvent accountOpened = new()
        {
            AccountId = 55,
            UserId = 99,
            AccountNumber = "SAV-0000000099"
        };

        Func<Task> act = () => PublishAsync(accountOpened);

        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Private Method Declarations

    private async Task PublishAsync(IMessage message)
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        IMessageDispatcher dispatcher = scope.ServiceProvider.GetRequiredService<IMessageDispatcher>();

        await dispatcher.PublishAsync(message, DispatchMode.Sequential, TestContext.Current.CancellationToken);
    }

    #endregion
}
