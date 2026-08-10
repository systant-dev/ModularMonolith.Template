using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;

namespace ModularMonolith.Template.SharedKernel.Tests.Outbox;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Configuration)]
public sealed class OutboxServiceCollectionExtensionsTests
{
    #region Test Method Declarations

    [Fact]
    public void AddOutboxStore_RegistersStoreAndInterface()
    {
        ServiceCollection services = new();

        services.AddOutboxStore<FakeStore>();

        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        FakeStore concrete = scope.ServiceProvider.GetRequiredService<FakeStore>();
        IOutboxStore viaInterface = scope.ServiceProvider.GetRequiredService<IOutboxStore>();

        viaInterface.Should().BeSameAs(concrete);
    }

    [Fact]
    public void AddOutboxStore_RegistersStoreTypeInRegistry()
    {
        ServiceCollection services = new();

        services.AddOutboxStore<FakeStore>();

        using ServiceProvider provider = services.BuildServiceProvider();
        OutboxStoreRegistry registry = provider.GetRequiredService<OutboxStoreRegistry>();

        registry.StoreTypes.Should().Contain(typeof(FakeStore));
    }

    [Fact]
    public void AddOutboxStore_CalledTwice_SharesSingleRegistry()
    {
        ServiceCollection services = new();

        services.AddOutboxStore<FakeStore>();
        services.AddOutboxStore<OtherStore>();

        using ServiceProvider provider = services.BuildServiceProvider();
        OutboxStoreRegistry registry = provider.GetRequiredService<OutboxStoreRegistry>();

        registry.StoreTypes.Should().BeEquivalentTo([typeof(FakeStore), typeof(OtherStore)]);
    }

    [Fact]
    public void AddOutboxProcessing_RegistersHostedServiceAndOptions()
    {
        ServiceCollection services = new();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Outbox:BatchSize"] = "42",
                ["Outbox:MaxAttempts"] = "7"
            })
            .Build();

        services.AddOutboxProcessing(configuration);

        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(IHostedService) &&
            descriptor.ImplementationType != null &&
            descriptor.ImplementationType.Name == "OutboxProcessor");

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<OutboxStoreRegistry>().Should().NotBeNull();

        OutboxOptions options = provider.GetRequiredService<
            Microsoft.Extensions.Options.IOptions<OutboxOptions>>().Value;
        options.BatchSize.Should().Be(42);
        options.MaxAttempts.Should().Be(7);
    }

    #endregion

    #region Nested Types

    private sealed class FakeStore : IOutboxStore
    {
        public Task EnqueueAsync(IMessage message, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxAttempts, int batchSize, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<OutboxMessage>>([]);

        public Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class OtherStore : IOutboxStore
    {
        public Task EnqueueAsync(IMessage message, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxAttempts, int batchSize, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<OutboxMessage>>([]);

        public Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    #endregion
}
