using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ModularMonolith.Template.SharedKernel.Messaging;

internal sealed class LoggingMessageHandlerDecorator<TMessage>(IMessageHandler<TMessage> inner,
                                                               ILogger<LoggingMessageHandlerDecorator<TMessage>> logger)
    : IMessageHandler<TMessage> where TMessage : IMessage
{
    #region Public Method Declarations

    public async Task HandleAsync(TMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        string handlerName = inner.GetType().Name;
        string messageType = typeof(TMessage).Name;
        long startTimestamp = Stopwatch.GetTimestamp();

        logger.LogHandling(messageType, handlerName);
        try
        {
            await inner.HandleAsync(message, cancellationToken);
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogHandled(messageType, handlerName, Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds);
            }
        }
        catch (Exception exception)
        {
            logger.LogHandlerFailed(handlerName, messageType, exception);
            throw;
        }
    }

    #endregion
}
