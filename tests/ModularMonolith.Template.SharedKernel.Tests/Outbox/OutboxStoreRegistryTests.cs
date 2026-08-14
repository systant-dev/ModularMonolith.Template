using FluentAssertions;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;

namespace ModularMonolith.Template.SharedKernel.Tests.Outbox;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Messaging)]
public sealed class OutboxStoreRegistryTests
{
    #region Test Method Declarations

    [Fact]
    public void Register_AddsStoreType()
    {
        OutboxStoreRegistry registry = new();

        registry.Register<FirstStore>();

        registry.StoreTypes.Should().ContainSingle().Which.Should().Be<FirstStore>();
    }

    [Fact]
    public void Register_IsIdempotentForSameType()
    {
        OutboxStoreRegistry registry = new();

        registry.Register<FirstStore>();
        registry.Register<FirstStore>();

        registry.StoreTypes.Should().ContainSingle();
    }

    [Fact]
    public void Register_TracksMultipleDistinctTypes()
    {
        OutboxStoreRegistry registry = new();

        registry.Register<FirstStore>();
        registry.Register<SecondStore>();

        registry.StoreTypes.Should().BeEquivalentTo([typeof(FirstStore), typeof(SecondStore)]);
    }

    #endregion

    #region Nested Types

    private sealed class FirstStore : IOutboxStore
    {
        public Task EnqueueAsync(IMessage message, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxAttempts, int batchSize, TimeSpan visibilityTimeout, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(IReadOnlyList<OutboxMessage> messages, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class SecondStore : IOutboxStore
    {
        public Task EnqueueAsync(IMessage message, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxAttempts, int batchSize, TimeSpan visibilityTimeout, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(IReadOnlyList<OutboxMessage> messages, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
