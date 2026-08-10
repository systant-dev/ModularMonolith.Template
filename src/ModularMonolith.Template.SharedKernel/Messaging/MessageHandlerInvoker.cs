using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Template.SharedKernel.Messaging;

public static class MessageHandlerInvoker
{
    #region Public Method Declarations

    public static async Task InvokeAsync<TMessage>(IServiceProvider serviceProvider, TMessage message, DispatchMode mode, CancellationToken cancellationToken)
        where TMessage : IMessage
    {
        IEnumerable<IMessageHandler<TMessage>> handlers = serviceProvider.GetServices<IMessageHandler<TMessage>>();

        if (mode == DispatchMode.Concurrent)
        {
            IEnumerable<Task> handlerTasks = handlers.Select(handler => handler.HandleAsync(message, cancellationToken));
            await Task.WhenAll(handlerTasks).ConfigureAwait(false);
            return;
        }

        foreach (IMessageHandler<TMessage> handler in handlers)
        {
            await handler.HandleAsync(message, cancellationToken).ConfigureAwait(false);
        }
    }

    #endregion
}
