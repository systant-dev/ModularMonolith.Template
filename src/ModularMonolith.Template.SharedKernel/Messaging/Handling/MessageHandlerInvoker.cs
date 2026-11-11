using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Messaging.Dispatch;

namespace ModularMonolith.Template.SharedKernel.Messaging.Handling;

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
            await Task.WhenAll(handlerTasks);
            return;
        }

        foreach (IMessageHandler<TMessage> handler in handlers)
        {
            await handler.HandleAsync(message, cancellationToken);
        }
    }

    #endregion
}
