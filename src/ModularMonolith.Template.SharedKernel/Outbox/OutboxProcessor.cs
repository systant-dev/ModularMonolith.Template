using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Persistence;

namespace ModularMonolith.Template.SharedKernel.Outbox;

internal sealed partial class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    OutboxContextRegistry registry,
    IOptions<OutboxOptions> options,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    #region Field Declarations

    private readonly OutboxOptions _options = options.Value;

    #endregion

    #region Protected Method Declarations

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(_options.PollingInterval);

        do
        {
            try
            {
                await ProcessAsync(stoppingToken).ConfigureAwait(false);
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
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
    }

    #endregion

    #region Private Method Declarations

    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        foreach (Type contextType in registry.ContextTypes)
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

            if (scope.ServiceProvider.GetRequiredService(contextType) is not ModuleDbContext context)
            {
                continue;
            }

            IMessageDispatcher dispatcher = scope.ServiceProvider.GetRequiredService<IMessageDispatcher>();

            List<OutboxMessage> pending = await context.OutboxMessages
                .Where(message => message.ProcessedOnUtc == null && message.Attempts < _options.MaxAttempts)
                .OrderBy(message => message.OccurredOnUtc)
                .Take(_options.BatchSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (pending.Count == 0)
            {
                continue;
            }

            foreach (OutboxMessage outboxMessage in pending)
            {
                await DispatchAsync(dispatcher, outboxMessage, cancellationToken).ConfigureAwait(false);
            }

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task DispatchAsync(IMessageDispatcher dispatcher, OutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        outboxMessage.Attempts++;

        try
        {
            IMessage? message = outboxMessage.Deserialise();
            if (message is null)
            {
                outboxMessage.Error = $"Unable to resolve message type '{outboxMessage.Type}'.";
                return;
            }

            await dispatcher.PublishAsync(message, cancellationToken).ConfigureAwait(false);

            outboxMessage.ProcessedOnUtc = DateTimeOffset.UtcNow;
            outboxMessage.Error = null;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            outboxMessage.Error = exception.Message;
            MessageDispatchFailed(logger, outboxMessage.Id, exception);
        }
    }

    #endregion

    #region Log Message Declarations

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "The outbox processing loop failed.")]
    private static partial void ProcessingLoopFailed(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Failed to dispatch outbox message {MessageId}.")]
    private static partial void MessageDispatchFailed(ILogger logger, Guid messageId, Exception exception);

    #endregion
}
