using Microsoft.Extensions.Logging;

namespace ModularMonolith.Template.SharedKernel.Messaging;

internal static partial class MessageHandlerLog
{
    #region Public Method Declarations

    [LoggerMessage(Level = LogLevel.Debug, Message = "Handling {MessageType} with {HandlerName}")]
    public static partial void LogHandling(this ILogger logger, string messageType, string handlerName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Handled {MessageType} with {HandlerName} in {ElapsedMilliseconds} ms")]
    public static partial void LogHandled(this ILogger logger, string messageType, string handlerName, double elapsedMilliseconds);

    [LoggerMessage(Level = LogLevel.Error, Message = "Handler {HandlerName} failed while handling {MessageType}")]
    public static partial void LogHandlerFailed(this ILogger logger, string handlerName, string messageType, Exception exception);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Skipping already-processed message for consumer {ConsumerName}, key {MessageKey}")]
    public static partial void LogDuplicateSkipped(this ILogger logger, string consumerName, string messageKey);

    #endregion
}
