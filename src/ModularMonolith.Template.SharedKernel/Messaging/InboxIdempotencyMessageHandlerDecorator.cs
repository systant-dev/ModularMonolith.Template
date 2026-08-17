using Microsoft.Extensions.Logging;
using ModularMonolith.Template.SharedKernel.Inbox;

namespace ModularMonolith.Template.SharedKernel.Messaging;

internal sealed class InboxIdempotencyMessageHandlerDecorator<TMessage>(IMessageHandler<TMessage> inner,
                                                                        IInboxStore inboxStore,
                                                                        string consumerName,
                                                                        ILogger<InboxIdempotencyMessageHandlerDecorator<TMessage>> logger)
    : IMessageHandler<TMessage> where TMessage : IMessage
{
    #region Public Method Declarations

    public async Task HandleAsync(TMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        string messageKey = message.MessageId.ToString();
        if (!await inboxStore.TryMarkProcessedAsync(consumerName, messageKey, cancellationToken))
        {
            logger.LogDuplicateSkipped(consumerName, messageKey);
            return;
        }

        await inner.HandleAsync(message, cancellationToken);
    }

    #endregion
}
