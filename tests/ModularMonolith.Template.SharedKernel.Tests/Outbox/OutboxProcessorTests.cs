using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;

namespace ModularMonolith.Template.SharedKernel.Tests.Outbox;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Messaging)]
public sealed class OutboxProcessorTests
{
    #region Test Method Declarations

    [Fact]
    public async Task Processes_DispatchesPendingMessagesAndMarksThemProcessed()
    {
        RecordingDispatcher dispatcher = new();
        FakeStore store = new();
        store.Add(OutboxMessage.Create(new TestMessage("first")));
        store.Add(OutboxMessage.Create(new TestMessage("second")));

        await RunProcessorAsync(store, dispatcher);

        dispatcher.Published.Should().HaveCount(2);
        store.Messages.Should().OnlyContain(message => message.ProcessedOnUtc != null);
        store.Messages.Should().OnlyContain(message => message.Attempts == 1);
        store.Messages.Should().OnlyContain(message => message.Error == null);
    }

    [Fact]
    public async Task Processes_WhenDispatchThrows_RecordsErrorAndIncrementsAttempts()
    {
        ThrowingDispatcher dispatcher = new();
        FakeStore store = new();
        store.Add(OutboxMessage.Create(new TestMessage("boom")));

        await RunProcessorAsync(store, dispatcher);

        OutboxMessage message = store.Messages.Should().ContainSingle().Subject;
        message.ProcessedOnUtc.Should().BeNull();
        message.Attempts.Should().Be(1);
        message.Error.Should().Be("dispatch failed");
    }

    [Fact]
    public async Task Processes_WhenMessageTypeCannotResolve_SetsError()
    {
        RecordingDispatcher dispatcher = new();
        FakeStore store = new();
        store.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "Some.Unknown.Type, Nonexistent.Assembly",
            Payload = "{}"
        });

        await RunProcessorAsync(store, dispatcher);

        OutboxMessage message = store.Messages.Should().ContainSingle().Subject;
        message.ProcessedOnUtc.Should().BeNull();
        message.Attempts.Should().Be(1);
        message.Error.Should().Contain("Unable to resolve message type");
        dispatcher.Published.Should().BeEmpty();
    }

    #endregion

    #region Private Method Declarations

    private static async Task RunProcessorAsync(FakeStore store, IMessageDispatcher dispatcher)
    {
        ServiceCollection services = new();
        services.AddSingleton(store);
        services.AddSingleton(dispatcher);
        using ServiceProvider provider = services.BuildServiceProvider();

        OutboxStoreRegistry registry = new();
        registry.Register(typeof(FakeStore));

        OutboxOptions options = new() { PollingInterval = TimeSpan.FromMilliseconds(20), MaxAttempts = 1 };

        OutboxProcessor processor = new(
            provider.GetRequiredService<IServiceScopeFactory>(),
            registry,
            Options.Create(options),
            NullLogger<OutboxProcessor>.Instance);

        await processor.StartAsync(TestContext.Current.CancellationToken);
        await WaitForAsync(() => store.Messages.All(message => message.Attempts > 0));
        await processor.StopAsync(TestContext.Current.CancellationToken);
    }

    private static async Task WaitForAsync(Func<bool> condition)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(5);
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(20);
        }
    }

    #endregion

    #region Nested Types

    public sealed record TestMessage(string Value) : IMessage;

    private sealed class FakeStore : IOutboxStore
    {
        private readonly ConcurrentDictionary<Guid, OutboxMessage> _messages = new();

        public IReadOnlyList<OutboxMessage> Messages => [.. _messages.Values];

        public void Add(OutboxMessage message) => _messages[message.Id] = message;

        public Task EnqueueAsync(IMessage message, CancellationToken cancellationToken = default)
        {
            Add(OutboxMessage.Create(message));
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxAttempts, int batchSize, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<OutboxMessage> pending =
            [
                .. _messages.Values
                    .Where(message => message.ProcessedOnUtc is null && message.Attempts < maxAttempts)
                    .OrderBy(message => message.OccurredOnUtc)
                    .Take(batchSize)
            ];

            return Task.FromResult(pending);
        }

        public Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            _messages[message.Id] = message;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingDispatcher : IMessageDispatcher
    {
        private readonly ConcurrentBag<IMessage> _published = [];

        public IReadOnlyCollection<IMessage> Published => _published;

        public Task PublishAsync(IMessage message, CancellationToken cancellationToken = default)
        {
            _published.Add(message);
            return Task.CompletedTask;
        }

        public Task PublishSequentiallyAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : IMessage =>
            PublishAsync(message, cancellationToken);

        public Task PublishConcurrentlyAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : IMessage =>
            PublishAsync(message, cancellationToken);
    }

    private sealed class ThrowingDispatcher : IMessageDispatcher
    {
        public Task PublishAsync(IMessage message, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("dispatch failed");

        public Task PublishSequentiallyAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : IMessage =>
            PublishAsync(message, cancellationToken);

        public Task PublishConcurrentlyAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : IMessage =>
            PublishAsync(message, cancellationToken);
    }

    #endregion
}
