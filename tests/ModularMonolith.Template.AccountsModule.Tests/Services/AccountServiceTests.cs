using AccountsModule.Contracts.Requests;
using AccountsModule.Contracts.Responses;
using AccountsModule.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using ModularMonolith.Template.AccountsModule.Tests.Storage;
using ModularMonolith.Template.SharedKernel.Outbox;

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

        AccountResponse created = await service.CreateAsync(NewRequest(7, "Grace Hopper", initialDeposit: 250m), TestContext.Current.CancellationToken);

        created.Id.Should().BePositive();
        created.UserId.Should().Be(7);
        created.AccountHolderName.Should().Be("Grace Hopper");
        created.AccountNumber.Should().NotBeNullOrWhiteSpace();
        created.Balance.Should().Be(250m);
    }

    [Fact]
    public async Task Create_EnqueuesOutboxMessage()
    {
        InMemoryAccountStore store = new();
        AccountService service = new(store, NullLogger<AccountService>.Instance);

        await service.CreateAsync(NewRequest(3, "Margaret Hamilton"), TestContext.Current.CancellationToken);

        IReadOnlyList<OutboxMessage> pending = await store.GetPendingAsync(maxAttempts: 5, batchSize: 10, visibilityTimeout: TimeSpan.FromMinutes(1), TestContext.Current.CancellationToken);

        pending.Should().ContainSingle();
    }

    [Fact]
    public async Task Find_WhenExists_ReturnsAccount()
    {
        AccountService service = CreateService();
        AccountResponse created = await service.CreateAsync(NewRequest(4, "Katherine Johnson"), TestContext.Current.CancellationToken);
        AccountResponse? found = await service.FindAsync(created.Id, TestContext.Current.CancellationToken);

        found.Should().NotBeNull();
        found!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task Find_WhenMissing_ReturnsNull()
    {
        AccountService service = CreateService();

        (await service.FindAsync(999, TestContext.Current.CancellationToken)).Should().BeNull();
    }

    [Fact]
    public async Task Update_WhenExists_ReturnsUpdatedAccount()
    {
        AccountService service = CreateService();
        AccountResponse created = await service.CreateAsync(NewRequest(5, "Original Name"), TestContext.Current.CancellationToken);

        AccountResponse? updated = await service.UpdateAsync(created.Id, new UpdateAccountRequest
        {
            AccountHolderName = "Updated Name"
        }, TestContext.Current.CancellationToken);

        updated.Should().NotBeNull();
        updated!.Id.Should().Be(created.Id);
        updated.AccountHolderName.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Update_WhenMissing_ReturnsNull()
    {
        AccountService service = CreateService();

        AccountResponse? updated = await service.UpdateAsync(999, new UpdateAccountRequest
        {
            AccountHolderName = "Nobody"
        }, TestContext.Current.CancellationToken);

        updated.Should().BeNull();
    }

    [Fact]
    public async Task Delete_WhenExists_ReturnsTrueAndRemoves()
    {
        AccountService service = CreateService();
        AccountResponse created = await service.CreateAsync(NewRequest(6, "Temp Holder"), TestContext.Current.CancellationToken);

        (await service.DeleteAsync(created.Id, TestContext.Current.CancellationToken)).Should().BeTrue();
        (await service.FindAsync(created.Id, TestContext.Current.CancellationToken)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_WhenMissing_ReturnsFalse()
    {
        AccountService service = CreateService();

        (await service.DeleteAsync(999, TestContext.Current.CancellationToken)).Should().BeFalse();
    }

    #endregion

    #region Private Method Declarations

    private static AccountService CreateService()
    {
        InMemoryAccountStore store = new();
        return new AccountService(store, NullLogger<AccountService>.Instance);
    }

    private static CreateAccountRequest NewRequest(int userId, string accountHolderName, decimal initialDeposit = 0m) =>
        new()
        {
            UserId = userId,
            AccountHolderName = accountHolderName,
            InitialDeposit = initialDeposit
        };

    #endregion
}
