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

    [Fact]
    public async Task Processes_WhenDispatchFailsBelowMaxAttempts_SchedulesBackoff()
    {
        ThrowingDispatcher dispatcher = new();
        FakeStore store = new();
        store.Add(OutboxMessage.Create(new TestMessage("retry-me")));

        OutboxOptions options = new()
        {
            PollingInterval = TimeSpan.FromMilliseconds(20),
            MaxAttempts = 3,
            BackoffBaseDelay = TimeSpan.FromMinutes(1),
            MaxBackoffDelay = TimeSpan.FromMinutes(5)
        };

        await RunProcessorAsync(store, dispatcher, options, () => store.Messages.All(message => message.Attempts > 0));

        OutboxMessage result = store.Messages.Should().ContainSingle().Subject;
        result.Attempts.Should().Be(1);
        result.ProcessedOnUtc.Should().BeNull();
        result.NextAttemptOnUtc.Should().NotBeNull();
        result.NextAttemptOnUtc.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Processes_WhenAttemptsExhausted_DeadLettersMessage()
    {
        ThrowingDispatcher dispatcher = new();
        FakeStore store = new();
        store.Add(OutboxMessage.Create(new TestMessage("dead")));

        OutboxOptions options = new() { PollingInterval = TimeSpan.FromMilliseconds(20), MaxAttempts = 1 };

        await RunProcessorAsync(store, dispatcher, options, () => store.Messages.All(message => message.Attempts >= 1));

        OutboxMessage result = store.Messages.Should().ContainSingle().Subject;
        result.Attempts.Should().Be(1);
        result.ProcessedOnUtc.Should().BeNull();
        result.NextAttemptOnUtc.Should().BeNull();
    }

    [Fact]
    public async Task Processes_WhenTransientFailure_RetriesUntilSuccess()
    {
        FailThenSucceedDispatcher dispatcher = new(failuresBeforeSuccess: 1);
        FakeStore store = new();
        store.Add(OutboxMessage.Create(new TestMessage("eventual")));

        OutboxOptions options = new()
        {
            PollingInterval = TimeSpan.FromMilliseconds(20),
            MaxAttempts = 3,
            BackoffBaseDelay = TimeSpan.FromMilliseconds(10),
            MaxBackoffDelay = TimeSpan.FromMilliseconds(50)
        };

        await RunProcessorAsync(store, dispatcher, options, () => store.Messages.All(message => message.ProcessedOnUtc != null));

        OutboxMessage result = store.Messages.Should().ContainSingle().Subject;
        result.ProcessedOnUtc.Should().NotBeNull();
        result.Attempts.Should().Be(2);
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task Processes_PersistsBatchWithSingleUpdateCall()
    {
        RecordingDispatcher dispatcher = new();
        FakeStore store = new();
        store.Add(OutboxMessage.Create(new TestMessage("first")));
        store.Add(OutboxMessage.Create(new TestMessage("second")));

        await RunProcessorAsync(store, dispatcher);

        store.UpdateCallCount.Should().Be(1);
    }

    #endregion

    #region Private Method Declarations

    private static async Task RunProcessorAsync(
        FakeStore store,
        IMessageDispatcher dispatcher,
        OutboxOptions? options = null,
        Func<bool>? stopWhen = null)
    {
        ServiceCollection services = new();
        services.AddSingleton(store);
        services.AddSingleton(dispatcher);
        using ServiceProvider provider = services.BuildServiceProvider();

        OutboxStoreRegistry registry = new();
        registry.Register(typeof(FakeStore));

        MessageContractRegistry contractRegistry = new();
        contractRegistry.Register(typeof(TestMessage));

        options ??= new OutboxOptions { PollingInterval = TimeSpan.FromMilliseconds(20), MaxAttempts = 1 };
        stopWhen ??= () => store.Messages.All(message => message.Attempts > 0);

        OutboxProcessor processor = new(
            provider.GetRequiredService<IServiceScopeFactory>(),
            registry,
            contractRegistry,
            Options.Create(options),
            NullLogger<OutboxProcessor>.Instance);

        await processor.StartAsync(TestContext.Current.CancellationToken);
        await WaitForAsync(stopWhen);
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
        private int _updateCallCount;

        public IReadOnlyList<OutboxMessage> Messages => [.. _messages.Values];

        public int UpdateCallCount => _updateCallCount;

        public void Add(OutboxMessage message) => _messages[message.Id] = message;

        public Task EnqueueAsync(IMessage message, CancellationToken cancellationToken = default)
        {
            Add(OutboxMessage.Create(message));
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxAttempts, int batchSize, TimeSpan visibilityTimeout, CancellationToken cancellationToken = default)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            IReadOnlyList<OutboxMessage> pending =
            [
                .. _messages.Values
                    .Where(message => message.ProcessedOnUtc is null
                        && message.Attempts < maxAttempts
                        && (message.NextAttemptOnUtc is null || message.NextAttemptOnUtc <= now))
                    .OrderBy(message => message.OccurredOnUtc)
                    .Take(batchSize)
            ];

            return Task.FromResult(pending);
        }

        public Task UpdateAsync(IReadOnlyList<OutboxMessage> messages, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _updateCallCount);

            foreach (OutboxMessage message in messages)
            {
                _messages[message.Id] = message;
            }

            return Task.CompletedTask;
        }
    }

    private sealed class RecordingDispatcher : IMessageDispatcher
    {
        private readonly ConcurrentBag<IMessage> _published = [];

        public IReadOnlyCollection<IMessage> Published => _published;

        public Task PublishAsync(IMessage message, DispatchMode mode = DispatchMode.Sequential, CancellationToken cancellationToken = default)
        {
            _published.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingDispatcher : IMessageDispatcher
    {
        public Task PublishAsync(IMessage message, DispatchMode mode = DispatchMode.Sequential, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("dispatch failed");
    }

    private sealed class FailThenSucceedDispatcher(int failuresBeforeSuccess) : IMessageDispatcher
    {
        private int _attempts;

        public Task PublishAsync(IMessage message, DispatchMode mode = DispatchMode.Sequential, CancellationToken cancellationToken = default)
        {
            if (Interlocked.Increment(ref _attempts) <= failuresBeforeSuccess)
            {
                throw new InvalidOperationException("transient failure");
            }

            return Task.CompletedTask;
        }
    }

    #endregion
}
