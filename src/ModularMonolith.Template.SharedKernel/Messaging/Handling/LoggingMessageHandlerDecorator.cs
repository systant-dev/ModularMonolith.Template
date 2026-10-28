using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ModularMonolith.Template.SharedKernel.Messaging.Handling;

internal sealed class LoggingMessageHandlerDecorator<TMessage>(IMessageHandler<TMessage> inner,
                                                               ILogger<LoggingMessageHandlerDecorator<TMessage>> logger)
    : IMessageHandler<TMessage> where TMessage : IMessage
{
    #region Field Declarations

    private static readonly string MessageTypeName = typeof(TMessage).Name;

    private readonly string _handlerName = inner.GetType().Name;

    #endregion

    #region Public Method Declarations

    public async Task HandleAsync(TMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        long startTimestamp = Stopwatch.GetTimestamp();

        logger.LogHandling(MessageTypeName, _handlerName);
        try
        {
            await inner.HandleAsync(message, cancellationToken);
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogHandled(MessageTypeName, _handlerName, Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds);
            }
        }
        catch (Exception exception)
        {
            logger.LogHandlerFailed(_handlerName, MessageTypeName, exception);
            throw;
        }
    }

    #endregion
}
