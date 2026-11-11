using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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
    public void AddOutboxStore_WhenRegistryRegisteredIncompatibly_Throws()
    {
        ServiceCollection services = new();
        services.AddSingleton(_ => new OutboxStoreRegistry());

        Action act = () => services.AddOutboxStore<FakeStore>();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddOutboxStore_WhenRegistryPreRegisteredAsInstance_ReusesInstance()
    {
        ServiceCollection services = new();
        OutboxStoreRegistry preRegistered = new();
        services.AddSingleton(preRegistered);

        services.AddOutboxStore<FakeStore>();

        using ServiceProvider provider = services.BuildServiceProvider();
        OutboxStoreRegistry resolved = provider.GetRequiredService<OutboxStoreRegistry>();

        resolved.Should().BeSameAs(preRegistered);
        resolved.StoreTypes.Should().Contain(typeof(FakeStore));
    }

    [Fact]
    public void AddOutboxProcessing_RegistersHostedServiceAndOptions()
    {
        ServiceCollection services = new();
        Dictionary<string, string?> configurationValues = new()
        {
            ["Outbox:BatchSize"] = "42",
            ["Outbox:MaxAttempts"] = "7"
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
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

    [Fact]
    public void AddOutboxProcessing_WithInvalidBatchSize_FailsValidation()
    {
        Dictionary<string, string?> configurationValues = new() { ["Outbox:BatchSize"] = "0" };
        ResolveOptions(configurationValues)
            .Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void AddOutboxProcessing_WithZeroPollingInterval_FailsValidation()
    {
        Dictionary<string, string?> configurationValues = new() { ["Outbox:PollingInterval"] = "00:00:00" };
        ResolveOptions(configurationValues)
            .Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void AddOutboxProcessing_WithMaxBackoffBelowBaseDelay_FailsValidation()
    {
        Dictionary<string, string?> configurationValues = new()
        {
            ["Outbox:BackoffBaseDelay"] = "00:01:00",
            ["Outbox:MaxBackoffDelay"] = "00:00:10"
        };
        ResolveOptions(configurationValues).Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void AddOutboxProcessing_WithValidConfiguration_PassesValidation()
    {
        Dictionary<string, string?> configurationValues = new() { ["Outbox:BatchSize"] = "10" };
        ResolveOptions(configurationValues)
            .Should().NotThrow();
    }

    #endregion

    #region Private Method Declarations

    private static Action ResolveOptions(Dictionary<string, string?> configurationValues)
    {
        ServiceCollection services = new();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        services.AddOutboxProcessing(configuration);

        ServiceProvider provider = services.BuildServiceProvider();
        return () => _ = provider.GetRequiredService<IOptions<OutboxOptions>>().Value;
    }

    #endregion

    #region Nested Types

    private sealed class FakeStore : IOutboxStore
    {
        public Task EnqueueAsync(IMessage message, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxAttempts, int batchSize, TimeSpan visibilityTimeout, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<OutboxMessage>>([]);

        public Task<IReadOnlyList<OutboxMessage>> GetDeadLetteredAsync(int maxAttempts, int batchSize, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<OutboxMessage>>([]);

        public Task UpdateAsync(IReadOnlyList<OutboxMessage> messages, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class OtherStore : IOutboxStore
    {
        public Task EnqueueAsync(IMessage message, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxAttempts, int batchSize, TimeSpan visibilityTimeout, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<OutboxMessage>>([]);

        public Task<IReadOnlyList<OutboxMessage>> GetDeadLetteredAsync(int maxAttempts, int batchSize, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<OutboxMessage>>([]);

        public Task UpdateAsync(IReadOnlyList<OutboxMessage> messages, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    #endregion
}
