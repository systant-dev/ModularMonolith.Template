using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using ModularMonolith.Template.UserModule.Tests.Infrastructure.Storage;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;
using ModularMonolith.Template.SharedKernel.Results;
using UserModule.Contracts.Events;
using UserModule.Contracts.Requests;
using UserModule.Contracts.Responses;
using UserModule.Features.Users;
using ModularMonolith.Template.SharedKernel.Messaging.Contracts;

namespace ModularMonolith.Template.UserModule.Tests.Features.Users;

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

        UserResponse created = (await service.CreateAsync(NewRequest("Grace Hopper", "grace@example.com", "555-0100"), TestContext.Current.CancellationToken)).Value;

        created.Id.Should().BePositive();
        created.FullName.Should().Be("Grace Hopper");
        created.Email.Should().Be("grace@example.com");
        created.PhoneNumber.Should().Be("555-0100");
    }

    [Fact]
    public async Task Create_WhenEmailAlreadyExists_ReturnsConflict()
    {
        UserService service = await CreateService();
        await service.CreateAsync(NewRequest("First User", "dupe@example.com"), TestContext.Current.CancellationToken);

        Result<UserResponse> result = await service.CreateAsync(NewRequest("Second User", "DUPE@example.com"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Create_EnqueuesOutboxMessage()
    {
        InMemoryUserStore store = new();
        UserService service = new(store, NullLogger<UserService>.Instance);

        UserResponse created = (await service.CreateAsync(NewRequest("Margaret Hamilton", "margaret@example.com"), TestContext.Current.CancellationToken)).Value;

        IReadOnlyList<OutboxMessage> pending =
            await store.GetPendingAsync(maxAttempts: 5, batchSize: 10, visibilityTimeout: TimeSpan.FromMinutes(1), TestContext.Current.CancellationToken);

        pending.Should().ContainSingle();

        UserCreatedIntegrationEvent published = DeserialiseSingle<UserCreatedIntegrationEvent>(pending);
        published.Id.Should().Be(created.Id);
        published.FullName.Should().Be("Margaret Hamilton");
        published.Email.Should().Be("margaret@example.com");
    }

    [Fact]
    public async Task Find_WhenExists_ReturnsUser()
    {
        UserService service = await CreateService();
        UserResponse created = (await service.CreateAsync(NewRequest("Katherine Johnson", "katherine@example.com"), TestContext.Current.CancellationToken)).Value;

        Result<UserResponse> found = await service.FindAsync(created.Id, TestContext.Current.CancellationToken);

        found.IsSuccess.Should().BeTrue();
        found.Value.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task Find_WhenMissing_ReturnsNotFound()
    {
        UserService service = await CreateService();

        Result<UserResponse> result = await service.FindAsync(999, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Update_WhenExists_ReturnsUpdatedUser()
    {
        UserService service = await CreateService();
        UserResponse created = (await service.CreateAsync(NewRequest("Original Name", "original@example.com"), TestContext.Current.CancellationToken)).Value;

        Result<UserResponse> updated = await service.UpdateAsync(created.Id, new UpdateUserRequest
        {
            FullName = "Updated Name",
            Email = "updated@example.com",
            PhoneNumber = "555-0199"
        }, TestContext.Current.CancellationToken);

        updated.IsSuccess.Should().BeTrue();
        updated.Value.Id.Should().Be(created.Id);
        updated.Value.FullName.Should().Be("Updated Name");
        updated.Value.Email.Should().Be("updated@example.com");
        updated.Value.PhoneNumber.Should().Be("555-0199");
    }

    [Fact]
    public async Task Update_WhenMissing_ReturnsNotFound()
    {
        UserService service = await CreateService();

        Result<UserResponse> updated = await service.UpdateAsync(999, new UpdateUserRequest
        {
            FullName = "Nobody",
            Email = "nobody@example.com"
        }, TestContext.Current.CancellationToken);

        updated.IsFailure.Should().BeTrue();
        updated.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Update_WhenEmailBelongsToAnotherUser_ReturnsConflict()
    {
        UserService service = await CreateService();
        await service.CreateAsync(NewRequest("First User", "taken@example.com"), TestContext.Current.CancellationToken);
        UserResponse second = (await service.CreateAsync(NewRequest("Second User", "second@example.com"), TestContext.Current.CancellationToken)).Value;

        Result<UserResponse> result = await service.UpdateAsync(second.Id, new UpdateUserRequest
        {
            FullName = "Second User",
            Email = "TAKEN@example.com"
        }, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Update_WhenKeepingOwnEmail_Succeeds()
    {
        UserService service = await CreateService();
        UserResponse created = (await service.CreateAsync(NewRequest("Same Email", "same@example.com"), TestContext.Current.CancellationToken)).Value;

        Result<UserResponse> result = await service.UpdateAsync(created.Id, new UpdateUserRequest
        {
            FullName = "Same Email Updated",
            Email = "same@example.com"
        }, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.FullName.Should().Be("Same Email Updated");
    }

    [Fact]
    public async Task Update_EnqueuesOutboxMessage()
    {
        InMemoryUserStore store = new();
        UserService service = new(store, NullLogger<UserService>.Instance);
        UserResponse created = (await service.CreateAsync(NewRequest("Update Event", "update-event@example.com"), TestContext.Current.CancellationToken)).Value;

        await service.UpdateAsync(created.Id, new UpdateUserRequest
        {
            FullName = "Update Event Changed",
            Email = "update-event-changed@example.com"
        }, TestContext.Current.CancellationToken);

        IReadOnlyList<OutboxMessage> pending =
            await store.GetPendingAsync(maxAttempts: 5, batchSize: 10, visibilityTimeout: TimeSpan.FromMinutes(1), TestContext.Current.CancellationToken);

        pending.Should().HaveCount(2);

        UserUpdatedIntegrationEvent published = DeserialiseSingle<UserUpdatedIntegrationEvent>(pending);
        published.Id.Should().Be(created.Id);
        published.FullName.Should().Be("Update Event Changed");
        published.Email.Should().Be("update-event-changed@example.com");
    }

    [Fact]
    public async Task Delete_WhenExists_SucceedsAndRemoves()
    {
        UserService service = await CreateService();
        UserResponse created = (await service.CreateAsync(NewRequest("Temp User", "temp@example.com"), TestContext.Current.CancellationToken)).Value;

        (await service.DeleteAsync(created.Id, TestContext.Current.CancellationToken)).IsSuccess.Should().BeTrue();
        Result<UserResponse> result = await service.FindAsync(created.Id, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Delete_EnqueuesOutboxMessage()
    {
        InMemoryUserStore store = new();
        UserService service = new(store, NullLogger<UserService>.Instance);
        UserResponse created = (await service.CreateAsync(NewRequest("Delete Event", "delete-event@example.com"), TestContext.Current.CancellationToken)).Value;

        await service.DeleteAsync(created.Id, TestContext.Current.CancellationToken);

        IReadOnlyList<OutboxMessage> pending =
            await store.GetPendingAsync(maxAttempts: 5, batchSize: 10, visibilityTimeout: TimeSpan.FromMinutes(1), TestContext.Current.CancellationToken);

        pending.Should().HaveCount(2);

        UserDeletedIntegrationEvent published = DeserialiseSingle<UserDeletedIntegrationEvent>(pending);
        published.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task Delete_WhenMissing_ReturnsNotFound()
    {
        UserService service = await CreateService();

        Result result = await service.DeleteAsync(999, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    #endregion

    #region Private Method Declarations

    private static Task<UserService> CreateService()
    {
        InMemoryUserStore store = new();
        UserService service = new(store, NullLogger<UserService>.Instance);
        return Task.FromResult(service);
    }

    private static CreateUserRequest NewRequest(string fullName, string email, string? phoneNumber = null)
    {
        CreateUserRequest request = new()
        {
            FullName = fullName,
            Email = email,
            PhoneNumber = phoneNumber
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
