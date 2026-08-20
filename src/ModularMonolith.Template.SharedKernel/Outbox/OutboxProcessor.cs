using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularMonolith.Template.SharedKernel.Messaging;
using System.Diagnostics;

namespace ModularMonolith.Template.SharedKernel.Outbox;

internal sealed partial class OutboxProcessor(IServiceScopeFactory scopeFactory,
                                              OutboxStoreRegistry registry,
                                              IMessageContractRegistry contractRegistry,
                                              IOptions<OutboxOptions> options,
                                              ILogger<OutboxProcessor> logger) : BackgroundService
{
    #region Field Declarations

    private readonly OutboxOptions _options = options.Value;
    private readonly long[] _backoffTicks = PrecomputeBackoff(options.Value);

    #endregion

    #region Protected Method Declarations

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(_options.PollingInterval);

        do
        {
            try
            {
                await ProcessAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                ProcessingLoopFailed(logger, exception);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));

    }

    #endregion

    #region Private Method Declarations

    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        if (_options.MaxDegreeOfParallelism <= 1)
        {
            foreach (Type storeType in registry.StoreTypes)
            {
                await ProcessStoreAsync(storeType, cancellationToken);
            }
            return;
        }

        ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism,
            CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(registry.StoreTypes, parallelOptions, async (storeType, token) => await ProcessStoreAsync(storeType, token));
    }

    private async Task ProcessStoreAsync(Type storeType, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

        if (scope.ServiceProvider.GetRequiredService(storeType) is not IOutboxStore store)
        {
            StoreTypeNotAnOutboxStore(logger, storeType);
            return;
        }

        IReadOnlyList<OutboxMessage> pending = await store.GetPendingAsync(_options.MaxAttempts, _options.BatchSize, _options.ClaimTimeout, cancellationToken);

        if (pending.Count == 0)
        {
            return;
        }

        if (_options.MaxDegreeOfParallelism <= 1)
        {
            IMessageDispatcher dispatcher = scope.ServiceProvider.GetRequiredService<IMessageDispatcher>();

            foreach (OutboxMessage outboxMessage in pending)
            {
                await DispatchAsync(dispatcher, outboxMessage, cancellationToken);
            }
        }
        else
        {
            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(pending, parallelOptions, DispatchInIsolatedScopeAsync);
        }

        await store.UpdateAsync(pending, cancellationToken);
    }

    private async ValueTask DispatchInIsolatedScopeAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        IMessageDispatcher dispatcher = scope.ServiceProvider.GetRequiredService<IMessageDispatcher>();
        await DispatchAsync(dispatcher, outboxMessage, cancellationToken);
    }

    private async Task DispatchAsync(IMessageDispatcher dispatcher, OutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        outboxMessage.Attempts++;

        using Activity? activity = OutboxDiagnostics.ActivitySource.StartActivity("outbox.dispatch", ActivityKind.Producer);
        activity?.SetTag("outbox.message.id", outboxMessage.Id);
        activity?.SetTag("outbox.message.type", outboxMessage.Type);
        activity?.SetTag("outbox.message.attempts", outboxMessage.Attempts);

        try
        {
            IMessage? message = outboxMessage.Deserialise(contractRegistry);
            if (message is null)
            {
                outboxMessage.Error = $"Unable to resolve message type '{outboxMessage.Type}'.";
                activity?.SetStatus(ActivityStatusCode.Error, outboxMessage.Error);
                HandleFailure(outboxMessage);
                return;
            }

            await dispatcher.PublishAsync(message, cancellationToken: cancellationToken);

            outboxMessage.ProcessedOnUtc = DateTimeOffset.UtcNow;
            outboxMessage.NextAttemptOnUtc = null;
            outboxMessage.Error = null;
            activity?.SetStatus(ActivityStatusCode.Ok);
            OutboxDiagnostics.ProcessedCounter.Add(1);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            outboxMessage.Error = exception.Message;
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            MessageDispatchFailed(logger, outboxMessage.Id, exception);
            HandleFailure(outboxMessage);
        }
    }

    private void HandleFailure(OutboxMessage outboxMessage)
    {
        OutboxDiagnostics.FailedCounter.Add(1);

        if (outboxMessage.Attempts >= _options.MaxAttempts)
        {
            outboxMessage.NextAttemptOnUtc = null;
            OutboxDiagnostics.DeadLetteredCounter.Add(1);
            MessageDeadLettered(logger, outboxMessage.Id, outboxMessage.Attempts);
            return;
        }

        outboxMessage.NextAttemptOnUtc = DateTimeOffset.UtcNow + CalculateBackoff(outboxMessage.Attempts);
        OutboxDiagnostics.RetriedCounter.Add(1);
    }

    private TimeSpan CalculateBackoff(int attempts)
    {
        int index = Math.Clamp(attempts, 1, _backoffTicks.Length) - 1;
        return TimeSpan.FromTicks(_backoffTicks[index]);
    }

    private static long[] PrecomputeBackoff(OutboxOptions options)
    {
        int count = Math.Max(options.MaxAttempts, 1);
        long[] ticks = new long[count];

        for (int attempt = 1; attempt <= count; attempt++)
        {
            double exponentialTicks = options.BackoffBaseDelay.Ticks * Math.Pow(2, attempt - 1);
            double cappedTicks = Math.Min(exponentialTicks, options.MaxBackoffDelay.Ticks);
            ticks[attempt - 1] = (long)cappedTicks;
        }

        return ticks;
    }

    #endregion

    #region Log Message Declarations

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "The outbox processing loop failed.")]
    private static partial void ProcessingLoopFailed(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Failed to dispatch outbox message {MessageId}.")]
    private static partial void MessageDispatchFailed(ILogger logger, Guid messageId, Exception exception);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Outbox message {MessageId} moved to dead-letter after {Attempts} attempts.")]
    private static partial void MessageDeadLettered(ILogger logger, Guid messageId, int attempts);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Registered outbox store type {StoreType} does not implement IOutboxStore and was skipped.")]
    private static partial void StoreTypeNotAnOutboxStore(ILogger logger, Type storeType);

    #endregion
}
