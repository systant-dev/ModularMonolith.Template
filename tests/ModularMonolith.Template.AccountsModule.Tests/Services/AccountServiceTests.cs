using AccountsModule.Contracts.Events;
using AccountsModule.Contracts.Requests;
using AccountsModule.Contracts.Responses;
using AccountsModule.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using ModularMonolith.Template.AccountsModule.Tests.Storage;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Messaging.Contracts;
using ModularMonolith.Template.SharedKernel.Outbox;
using ModularMonolith.Template.SharedKernel.Results;

namespace ModularMonolith.Template.AccountsModule.Tests.Services;

[Component(TestComponent.AccountsModule)]
[Category(TestCategory.Modular)]
public sealed class AccountServiceTests
{
    #region Test Method Declarations

    [Fact]
    public async Task GetAll_WhenEmpty_ReturnsEmptyCollection()
    {
        AccountService service = CreateService();

        (await service.GetAllAsync(TestContext.Current.CancellationToken)).Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_ReturnsCreatedAccounts()
    {
        AccountService service = CreateService();
        await service.CreateAsync(NewRequest(1, "Ada Lovelace"), TestContext.Current.CancellationToken);
        await service.CreateAsync(NewRequest(2, "Alan Turing"), TestContext.Current.CancellationToken);

        IReadOnlyCollection<AccountResponse> all = await service.GetAllAsync(TestContext.Current.CancellationToken);

        all.Should().HaveCount(2);
        all.Select(account => account.AccountHolderName).Should().Contain(["Ada Lovelace", "Alan Turing"]);
    }

    [Fact]
    public async Task Create_AssignsIdAccountNumberAndBalance()
    {
        AccountService service = CreateService();

        AccountResponse created = (await service.CreateAsync(NewRequest(7, "Grace Hopper", initialDeposit: 250m), TestContext.Current.CancellationToken)).Value;

        created.Id.Should().BePositive();
        created.UserId.Should().Be(7);
        created.AccountHolderName.Should().Be("Grace Hopper");
        created.AccountNumber.Should().NotBeNullOrWhiteSpace();
        created.Balance.Should().Be(250m);
    }

    [Fact]
    public async Task Create_WhenAccountAlreadyExistsForUser_ReturnsConflict()
    {
        AccountService service = CreateService();
        await service.CreateAsync(NewRequest(42, "First Holder"), TestContext.Current.CancellationToken);

        Result<AccountResponse> result = await service.CreateAsync(NewRequest(42, "Second Holder"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Create_EnqueuesOutboxMessage()
    {
        InMemoryAccountStore store = new();
        AccountService service = new(store, NullLogger<AccountService>.Instance);

        AccountResponse created = (await service.CreateAsync(NewRequest(3, "Margaret Hamilton"), TestContext.Current.CancellationToken)).Value;

        IReadOnlyList<OutboxMessage> pending = await store.GetPendingAsync(maxAttempts: 5, batchSize: 10, visibilityTimeout: TimeSpan.FromMinutes(1), TestContext.Current.CancellationToken);

        pending.Should().ContainSingle();

        AccountOpenedIntegrationEvent published = DeserialiseSingle<AccountOpenedIntegrationEvent>(pending);
        published.AccountId.Should().Be(created.Id);
        published.UserId.Should().Be(3);
        published.AccountNumber.Should().Be(created.AccountNumber);
    }

    [Fact]
    public async Task Find_WhenExists_ReturnsAccount()
    {
        AccountService service = CreateService();
        AccountResponse created = (await service.CreateAsync(NewRequest(4, "Katherine Johnson"), TestContext.Current.CancellationToken)).Value;
        Result<AccountResponse> found = await service.FindAsync(created.Id, TestContext.Current.CancellationToken);

        found.IsSuccess.Should().BeTrue();
        found.Value.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task Find_WhenMissing_ReturnsNotFound()
    {
        AccountService service = CreateService();

        Result<AccountResponse> result = await service.FindAsync(999, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Update_WhenExists_ReturnsUpdatedAccount()
    {
        AccountService service = CreateService();
        AccountResponse created = (await service.CreateAsync(NewRequest(5, "Original Name"), TestContext.Current.CancellationToken)).Value;

        Result<AccountResponse> updated = await service.UpdateAsync(created.Id, new UpdateAccountRequest
        {
            AccountHolderName = "Updated Name"
        }, TestContext.Current.CancellationToken);

        updated.IsSuccess.Should().BeTrue();
        updated.Value.Id.Should().Be(created.Id);
        updated.Value.AccountHolderName.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Update_WhenMissing_ReturnsNotFound()
    {
        AccountService service = CreateService();

        Result<AccountResponse> updated = await service.UpdateAsync(999, new UpdateAccountRequest
        {
            AccountHolderName = "Nobody"
        }, TestContext.Current.CancellationToken);

        updated.IsFailure.Should().BeTrue();
        updated.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Delete_WhenExists_SucceedsAndRemoves()
    {
        AccountService service = CreateService();
        AccountResponse created = (await service.CreateAsync(NewRequest(6, "Temp Holder"), TestContext.Current.CancellationToken)).Value;

        (await service.DeleteAsync(created.Id, TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
        Result<AccountResponse> result = await service.FindAsync(created.Id, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Delete_WhenMissing_ReturnsNotFound()
    {
        AccountService service = CreateService();

        Result result = await service.DeleteAsync(999, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Create_WhenAccountNumberCollides_RegeneratesAndSucceeds()
    {
        InMemoryAccountStore store = new() { AccountNumberCollisionsToSimulate = 2 };
        AccountService service = CreateService(store);

        Result<AccountResponse> result = await service.CreateAsync(NewRequest(11, "Katherine Johnson"), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccountNumber.Should().NotBeNullOrWhiteSpace();
        store.AccountNumberCollisionsToSimulate.Should().Be(0);
    }

    [Fact]
    public async Task Create_WhenAccountNumberCollisionPersists_ReturnsConflict()
    {
        InMemoryAccountStore store = new() { AccountNumberCollisionsToSimulate = 100 };
        AccountService service = CreateService(store);

        Result<AccountResponse> result = await service.CreateAsync(NewRequest(12, "Dorothy Vaughan"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("account.number-generation-failed");
    }

    #endregion

    #region Private Method Declarations

    private static AccountService CreateService()
    {
        return CreateService(new InMemoryAccountStore());
    }

    private static AccountService CreateService(InMemoryAccountStore store)
    {
        return new AccountService(store, NullLogger<AccountService>.Instance);
    }

    private static CreateAccountRequest NewRequest(int userId, string accountHolderName, decimal initialDeposit = 0m)
    {
        CreateAccountRequest request = new()
        {
            UserId = userId,
            AccountHolderName = accountHolderName,
            InitialDeposit = initialDeposit
        };

        return request;
    }

    private static TEvent DeserialiseSingle<TEvent>(IReadOnlyList<OutboxMessage> messages)
        where TEvent : class, IMessage
    {
        MessageContractRegistry registry = new();
        registry.Register<TEvent>();

        return messages
            .Select(message => message.Deserialise(registry))
            .OfType<TEvent>()
            .Should().ContainSingle()
            .Subject;
    }

    #endregion
}
