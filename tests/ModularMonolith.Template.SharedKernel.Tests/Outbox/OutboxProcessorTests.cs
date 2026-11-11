using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ModularMonolith.Template.SharedKernel.Inbox;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Messaging.Contracts;
using ModularMonolith.Template.SharedKernel.Messaging.Dispatch;
using ModularMonolith.Template.SharedKernel.Outbox;
using Moq;
using System.Collections.Concurrent;

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
        TestMessage firstMessage = new("first");
        TestMessage secondMessage = new("second");
        store.Add(OutboxMessage.Create(firstMessage));
        store.Add(OutboxMessage.Create(secondMessage));

        await RunProcessorAsync(store, dispatcher);

        dispatcher.Published.Should().HaveCount(2);
        store.Messages.Should().OnlyContain(message => message.ProcessedOnUtc != null);
        store.Messages.Should().OnlyContain(message => message.Attempts == 1);
        store.Messages.Should().OnlyContain(message => message.Error == null);
    }

    [Fact]
    public async Task Processes_WhenDispatchThrows_RecordsErrorAndIncrementsAttempts()
    {
        IMessageDispatcher dispatcher = CreateThrowingDispatcher();
        FakeStore store = new();
        TestMessage boomMessage = new("boom");
        store.Add(OutboxMessage.Create(boomMessage));

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
        OutboxMessage unresolvableMessage = new()
        {
            Id = Guid.CreateVersion7(),
            Type = "Some.Unknown.Type, Nonexistent.Assembly",
            Payload = "{}"
        };
        store.Add(unresolvableMessage);

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
        IMessageDispatcher dispatcher = CreateThrowingDispatcher();
        FakeStore store = new();
        TestMessage retryMessage = new("retry-me");
        store.Add(OutboxMessage.Create(retryMessage));

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
        IMessageDispatcher dispatcher = CreateThrowingDispatcher();
        FakeStore store = new();
        TestMessage deadMessage = new("dead");
        store.Add(OutboxMessage.Create(deadMessage));

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
        TestMessage eventualMessage = new("eventual");
        store.Add(OutboxMessage.Create(eventualMessage));

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
        TestMessage firstMessage = new("first");
        TestMessage secondMessage = new("second");
        store.Add(OutboxMessage.Create(firstMessage));
        store.Add(OutboxMessage.Create(secondMessage));

        await RunProcessorAsync(store, dispatcher);

        store.UpdateCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Processes_WhenHandlerTimesOut_RecordsFailureAndCompletesRestOfBatch()
    {
        ClaimingStore store = new();
        store.Add(OutboxMessage.Create(new TestMessage("first")));
        store.Add(OutboxMessage.Create(new TestMessage("timeout")));
        store.Add(OutboxMessage.Create(new TestMessage("last")));

        DelegateDispatcher dispatcher = new((message, _) => message is TestMessage { Value: "timeout" }
            ? throw new TaskCanceledException("handler timed out")
            : Task.CompletedTask);

        OutboxOptions options = new() { PollingInterval = TimeSpan.FromMilliseconds(20), MaxAttempts = 1 };
        using ServiceProvider provider = BuildProvider(store, dispatcher);
        OutboxProcessor processor = CreateProcessor<ClaimingStore>(provider, options);

        await processor.StartAsync(TestContext.Current.CancellationToken);
        await WaitForAsync(() => store.Rows.All(row => row.ProcessedOnUtc is not null || row.Attempts > 0));
        await processor.StopAsync(TestContext.Current.CancellationToken);

        IReadOnlyList<OutboxMessage> rows = store.Rows;
        rows[0].ProcessedOnUtc.Should().NotBeNull();
        rows[2].ProcessedOnUtc.Should().NotBeNull();
        rows[1].ProcessedOnUtc.Should().BeNull();
        rows[1].Attempts.Should().Be(1);
        rows[1].Error.Should().Be("handler timed out");
    }

    [Fact]
    public async Task Stop_MidBatch_PersistsDispatchedMessagesAndReleasesTheRest()
    {
        ClaimingStore store = new();
        store.Add(OutboxMessage.Create(new TestMessage("first")));
        store.Add(OutboxMessage.Create(new TestMessage("second")));
        store.Add(OutboxMessage.Create(new TestMessage("third")));

        TaskCompletionSource secondStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        DelegateDispatcher dispatcher = new(async (message, cancellationToken) =>
        {
            if (message is TestMessage { Value: "second" })
            {
                secondStarted.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
        });

        OutboxOptions options = new() { PollingInterval = TimeSpan.FromMilliseconds(20), MaxAttempts = 3 };
        using ServiceProvider provider = BuildProvider(store, dispatcher);
        OutboxProcessor processor = CreateProcessor<ClaimingStore>(provider, options);

        await processor.StartAsync(TestContext.Current.CancellationToken);
        await secondStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await processor.StopAsync(TestContext.Current.CancellationToken);

        IReadOnlyList<OutboxMessage> rows = store.Rows;
        rows[0].ProcessedOnUtc.Should().NotBeNull("it was dispatched before the host stopped");
        rows[0].Attempts.Should().Be(1);

        rows.Skip(1).Should().AllSatisfy(row =>
        {
            row.ProcessedOnUtc.Should().BeNull();
            row.Attempts.Should().Be(0, "an attempt interrupted by shutdown is not a failed attempt");
            row.NextAttemptOnUtc.Should().BeNull("the claim is released so the message is picked up on the next start");
        });
    }

    [Fact]
    public async Task Purges_ProcessedMessagesOlderThanRetentionPeriod()
    {
        ClaimingStore store = new();
        OutboxMessage expired = CreateProcessedMessage("expired", processedAgo: TimeSpan.FromDays(8));
        OutboxMessage recent = CreateProcessedMessage("recent", processedAgo: TimeSpan.FromDays(1));
        OutboxMessage pending = OutboxMessage.Create(new TestMessage("pending"));
        store.Add(expired);
        store.Add(recent);
        store.Add(pending);

        OutboxOptions options = new() { PollingInterval = TimeSpan.FromMilliseconds(20), RetentionPeriod = TimeSpan.FromDays(7), PurgeBatchSize = 1 };
        using ServiceProvider provider = BuildProvider(store, new RecordingDispatcher());
        OutboxProcessor processor = CreateProcessor<ClaimingStore>(provider, options);

        await processor.StartAsync(TestContext.Current.CancellationToken);
        await WaitForAsync(() => store.Rows.Count == 2);
        await processor.StopAsync(TestContext.Current.CancellationToken);

        store.Rows.Select(row => row.Id).Should().BeEquivalentTo([recent.Id, pending.Id]);
    }

    [Fact]
    public async Task Purges_NothingWhenRetentionPeriodIsZero()
    {
        ClaimingStore store = new();
        store.Add(CreateProcessedMessage("expired", processedAgo: TimeSpan.FromDays(400)));

        OutboxOptions options = new() { PollingInterval = TimeSpan.FromMilliseconds(20), RetentionPeriod = TimeSpan.Zero };
        using ServiceProvider provider = BuildProvider(store, new RecordingDispatcher());
        OutboxProcessor processor = CreateProcessor<ClaimingStore>(provider, options);

        await processor.StartAsync(TestContext.Current.CancellationToken);
        await Task.Delay(TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken);
        await processor.StopAsync(TestContext.Current.CancellationToken);

        store.Rows.Should().ContainSingle();
    }

    [Fact]
    public async Task Purges_EveryKeyedInboxStore()
    {
        RecordingInboxStore firstInboxStore = new();
        RecordingInboxStore secondInboxStore = new();
        ServiceCollection services = new();
        services.AddSingleton(new ClaimingStore());
        services.AddKeyedSingleton<IInboxStore>("first", firstInboxStore);
        services.AddKeyedSingleton<IInboxStore>("second", secondInboxStore);
        using ServiceProvider provider = services.BuildServiceProvider();

        OutboxOptions options = new() { PollingInterval = TimeSpan.FromMilliseconds(20), RetentionPeriod = TimeSpan.FromDays(7), PurgeBatchSize = 10 };
        OutboxProcessor processor = CreateProcessor<ClaimingStore>(provider, options);

        await processor.StartAsync(TestContext.Current.CancellationToken);
        await WaitForAsync(() => !firstInboxStore.PurgeBatchSizes.IsEmpty && !secondInboxStore.PurgeBatchSizes.IsEmpty);
        await processor.StopAsync(TestContext.Current.CancellationToken);

        firstInboxStore.PurgeBatchSizes.Should().Equal(10);
        secondInboxStore.PurgeBatchSizes.Should().Equal(10);
    }

    #endregion

    #region Private Method Declarations

    private static OutboxMessage CreateProcessedMessage(string value, TimeSpan processedAgo)
    {
        OutboxMessage message = OutboxMessage.Create(new TestMessage(value));
        message.ProcessedOnUtc = DateTimeOffset.UtcNow - processedAgo;
        message.Attempts = 1;
        return message;
    }

    private static async Task RunProcessorAsync(FakeStore store, IMessageDispatcher dispatcher, OutboxOptions? options = null, Func<bool>? stopWhen = null)
    {
        ServiceCollection services = new();
        services.AddSingleton(store);
        services.AddSingleton(dispatcher);
        using ServiceProvider provider = services.BuildServiceProvider();

        OutboxStoreRegistry registry = new();
        registry.Register<FakeStore>();

        MessageContractRegistry contractRegistry = new();
        contractRegistry.Register<TestMessage>();

        options ??= new OutboxOptions { PollingInterval = TimeSpan.FromMilliseconds(20), MaxAttempts = 1 };
        stopWhen ??= () => store.Messages.All(message => message.Attempts > 0);

        IServiceScopeFactory scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        OutboxProcessor processor = new(scopeFactory, registry, contractRegistry, Options.Create(options), NullLogger<OutboxProcessor>.Instance);

        await processor.StartAsync(TestContext.Current.CancellationToken);
        await WaitForAsync(stopWhen);
        await processor.StopAsync(TestContext.Current.CancellationToken);
    }

    private static ServiceProvider BuildProvider<TStore>(TStore store, IMessageDispatcher dispatcher) where TStore : class, IOutboxStore
    {
        ServiceCollection services = new();
        services.AddSingleton(store);
        services.AddSingleton(dispatcher);
        return services.BuildServiceProvider();
    }

    private static OutboxProcessor CreateProcessor<TStore>(ServiceProvider provider, OutboxOptions options) where TStore : class, IOutboxStore
    {
        OutboxStoreRegistry registry = new();
        registry.Register<TStore>();

        MessageContractRegistry contractRegistry = new();
        contractRegistry.Register<TestMessage>();

        IServiceScopeFactory scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        return new OutboxProcessor(scopeFactory, registry, contractRegistry, Options.Create(options), NullLogger<OutboxProcessor>.Instance);
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

    private static IMessageDispatcher CreateThrowingDispatcher()
    {
        Mock<IMessageDispatcher> dispatcher = new();
        InvalidOperationException dispatchException = new("dispatch failed");
        dispatcher.Setup(instance => instance.PublishAsync(It.IsAny<IMessage>(), It.IsAny<DispatchMode>(), It.IsAny<CancellationToken>()))
                  .ThrowsAsync(dispatchException);
        return dispatcher.Object;
    }

    #endregion

    #region Nested Types

    public sealed record TestMessage(string Value) : Message;

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

        public Task<IReadOnlyList<OutboxMessage>> GetDeadLetteredAsync(int maxAttempts, int batchSize, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<OutboxMessage> deadLettered =
            [
                .. _messages.Values
                    .Where(message => message.ProcessedOnUtc is null && message.Attempts >= maxAttempts)
                    .OrderBy(message => message.OccurredOnUtc)
                    .Take(batchSize)
            ];

            return Task.FromResult(deadLettered);
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

    private sealed class ClaimingStore : IOutboxStore
    {
        private readonly Lock _gate = new();
        private readonly List<OutboxMessage> _rows = [];

        public IReadOnlyList<OutboxMessage> Rows
        {
            get
            {
                lock (_gate)
                {
                    return [.. _rows];
                }
            }
        }

        public void Add(OutboxMessage message)
        {
            lock (_gate)
            {
                _rows.Add(message);
            }
        }

        public Task EnqueueAsync(IMessage message, CancellationToken cancellationToken = default)
        {
            Add(OutboxMessage.Create(message));
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxAttempts, int batchSize, TimeSpan visibilityTimeout, CancellationToken cancellationToken = default)
        {
            lock (_gate)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                List<OutboxMessage> claimed = [];

                foreach (OutboxMessage row in _rows)
                {
                    bool claimable = row.ProcessedOnUtc is null
                        && row.Attempts < maxAttempts
                        && (row.NextAttemptOnUtc is null || row.NextAttemptOnUtc <= now);

                    if (claimable && claimed.Count < batchSize)
                    {
                        row.NextAttemptOnUtc = now + visibilityTimeout;
                        claimed.Add(Copy(row));
                    }
                }

                return Task.FromResult<IReadOnlyList<OutboxMessage>>(claimed);
            }
        }

        public Task<IReadOnlyList<OutboxMessage>> GetDeadLetteredAsync(int maxAttempts, int batchSize, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<OutboxMessage>>([]);
        }

        public Task UpdateAsync(IReadOnlyList<OutboxMessage> messages, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_gate)
            {
                foreach (OutboxMessage message in messages)
                {
                    int index = _rows.FindIndex(row => row.Id == message.Id);
                    _rows[index] = Copy(message);
                }
            }

            return Task.CompletedTask;
        }

        public Task<int> PurgeProcessedAsync(DateTimeOffset processedBefore, int batchSize, CancellationToken cancellationToken = default)
        {
            lock (_gate)
            {
                List<OutboxMessage> expired = [.. _rows.Where(row => row.ProcessedOnUtc < processedBefore).Take(batchSize)];
                expired.ForEach(row => _rows.Remove(row));
                return Task.FromResult(expired.Count);
            }
        }

        private static OutboxMessage Copy(OutboxMessage source)
        {
            return new OutboxMessage
            {
                Id = source.Id,
                Type = source.Type,
                Payload = source.Payload,
                OccurredOnUtc = source.OccurredOnUtc,
                ProcessedOnUtc = source.ProcessedOnUtc,
                Attempts = source.Attempts,
                Error = source.Error,
                NextAttemptOnUtc = source.NextAttemptOnUtc
            };
        }
    }

    private sealed class RecordingInboxStore : IInboxStore
    {
        public ConcurrentQueue<int> PurgeBatchSizes { get; } = [];

        public Task<bool> IsProcessedAsync(string consumer, string messageId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<bool> TryMarkProcessedAsync(string consumer, string messageId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<int> PurgeProcessedAsync(DateTimeOffset processedBefore, int batchSize, CancellationToken cancellationToken = default)
        {
            PurgeBatchSizes.Enqueue(batchSize);
            return Task.FromResult(0);
        }
    }

    private sealed class DelegateDispatcher(Func<IMessage, CancellationToken, Task> publish) : IMessageDispatcher
    {
        public Task PublishAsync(IMessage message, DispatchMode mode = DispatchMode.Sequential, CancellationToken cancellationToken = default)
        {
            return publish(message, cancellationToken);
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
