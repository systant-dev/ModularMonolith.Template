using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using ModularMonolith.Template.UserModule.Tests.Storage;
using UserModule.Contracts.Requests;
using UserModule.Contracts.Responses;
using UserModule.Services;

namespace ModularMonolith.Template.UserModule.Tests.Services;

[Component(TestComponent.UserModule)]
[Category(TestCategory.Modular)]
public sealed class UserServiceTests
{
    #region Test Method Declarations

    [Fact]
    public async Task GetAll_WhenEmpty_ReturnsEmptyCollection()
    {
        UserService service = await CreateService();

        (await service.GetAllAsync(TestContext.Current.CancellationToken)).Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_ReturnsCreatedUsers()
    {
        UserService service = await CreateService();
        await service.CreateAsync(NewRequest("Ada Lovelace", "ada@example.com"), TestContext.Current.CancellationToken);
        await service.CreateAsync(NewRequest("Alan Turing", "alan@example.com"), TestContext.Current.CancellationToken);

        IReadOnlyCollection<UserResponse> all = await service.GetAllAsync(TestContext.Current.CancellationToken);

        all.Should().HaveCount(2);
        all.Select(user => user.FullName).Should().Contain(["Ada Lovelace", "Alan Turing"]);
    }

    [Fact]
    public async Task Create_AssignsIdAndMapsFields()
    {
        UserService service = await CreateService();

        UserResponse created = await service.CreateAsync(NewRequest("Grace Hopper", "grace@example.com", "555-0100"), TestContext.Current.CancellationToken);

        created.Id.Should().BePositive();
        created.FullName.Should().Be("Grace Hopper");
        created.Email.Should().Be("grace@example.com");
        created.PhoneNumber.Should().Be("555-0100");
    }

    [Fact]
    public async Task Create_EnqueuesOutboxMessage()
    {
        InMemoryUserStore store = new();
        UserService service = new(store, NullLogger<UserService>.Instance);

        await service.CreateAsync(NewRequest("Margaret Hamilton", "margaret@example.com"), TestContext.Current.CancellationToken);

        IReadOnlyList<ModularMonolith.Template.SharedKernel.Outbox.OutboxMessage> pending =
            await store.GetPendingAsync(maxAttempts: 5, batchSize: 10, visibilityTimeout: TimeSpan.FromMinutes(1), TestContext.Current.CancellationToken);

        pending.Should().ContainSingle();
    }

    [Fact]
    public async Task Find_WhenExists_ReturnsUser()
    {
        UserService service = await CreateService();
        UserResponse created = await service.CreateAsync(NewRequest("Katherine Johnson", "katherine@example.com"), TestContext.Current.CancellationToken);

        UserResponse? found = await service.FindAsync(created.Id, TestContext.Current.CancellationToken);

        found.Should().NotBeNull();
        found!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task Find_WhenMissing_ReturnsNull()
    {
        UserService service = await CreateService();

        (await service.FindAsync(999, TestContext.Current.CancellationToken)).Should().BeNull();
    }

    [Fact]
    public async Task Update_WhenExists_ReturnsUpdatedUser()
    {
        UserService service = await CreateService();
        UserResponse created = await service.CreateAsync(NewRequest("Original Name", "original@example.com"), TestContext.Current.CancellationToken);

        UserResponse? updated = await service.UpdateAsync(created.Id, new UpdateUserRequest
        {
            FullName = "Updated Name",
            Email = "updated@example.com",
            PhoneNumber = "555-0199"
        }, TestContext.Current.CancellationToken);

        updated.Should().NotBeNull();
        updated!.Id.Should().Be(created.Id);
        updated.FullName.Should().Be("Updated Name");
        updated.Email.Should().Be("updated@example.com");
        updated.PhoneNumber.Should().Be("555-0199");
    }

    [Fact]
    public async Task Update_WhenMissing_ReturnsNull()
    {
        UserService service = await CreateService();

        UserResponse? updated = await service.UpdateAsync(999, new UpdateUserRequest
        {
            FullName = "Nobody",
            Email = "nobody@example.com"
        }, TestContext.Current.CancellationToken);

        updated.Should().BeNull();
    }

    [Fact]
    public async Task Delete_WhenExists_ReturnsTrueAndRemoves()
    {
        UserService service = await CreateService();
        UserResponse created = await service.CreateAsync(NewRequest("Temp User", "temp@example.com"), TestContext.Current.CancellationToken);

        (await service.DeleteAsync(created.Id, TestContext.Current.CancellationToken)).Should().BeTrue();
        (await service.FindAsync(created.Id, TestContext.Current.CancellationToken)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_WhenMissing_ReturnsFalse()
    {
        UserService service = await CreateService();

        (await service.DeleteAsync(999, TestContext.Current.CancellationToken)).Should().BeFalse();
    }

    #endregion

    #region Private Method Declarations

    private static Task<UserService> CreateService()
    {
        InMemoryUserStore store = new();
        UserService service = new(store, NullLogger<UserService>.Instance);
        return Task.FromResult(service);
    }

    private static CreateUserRequest NewRequest(string fullName, string email, string? phoneNumber = null) =>
        new()
        {
            FullName = fullName,
            Email = email,
            PhoneNumber = phoneNumber
        };

    #endregion
}
