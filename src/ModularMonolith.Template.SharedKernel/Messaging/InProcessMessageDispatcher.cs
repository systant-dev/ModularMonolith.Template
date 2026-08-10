using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;

namespace ModularMonolith.Template.SharedKernel.Messaging;

internal sealed class InProcessMessageDispatcher(IServiceProvider serviceProvider) : IMessageDispatcher
{
    #region Field Declarations

    private static readonly ConcurrentDictionary<(Type MessageType, DispatchMode Mode), MethodInfo> PublishMethodCache = new();

    #endregion

    #region Public Method Declarations

    public Task PublishAsync(IMessage message, DispatchMode mode = DispatchMode.Sequential, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        MethodInfo method = PublishMethodCache.GetOrAdd((message.GetType(), mode), static key =>
            typeof(InProcessMessageDispatcher)
                .GetMethod(
                    key.Mode == DispatchMode.Concurrent ? nameof(PublishConcurrentlyAsync) : nameof(PublishSequentiallyAsync),
                    BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(key.MessageType));

        return (Task)method.Invoke(this, [message, cancellationToken])!;
    }

    #endregion

    #region Private Method Declarations

    private async Task PublishSequentiallyAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : IMessage
    {
        IEnumerable<IMessageHandler<TMessage>> handlers = serviceProvider.GetServices<IMessageHandler<TMessage>>();

        foreach (IMessageHandler<TMessage> handler in handlers)
        {
            await handler.HandleAsync(message, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task PublishConcurrentlyAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : IMessage
    {
        IEnumerable<IMessageHandler<TMessage>> handlers = serviceProvider.GetServices<IMessageHandler<TMessage>>();

        IEnumerable<Task> handlerTasks = handlers.Select(handler => handler.HandleAsync(message, cancellationToken));

        await Task.WhenAll(handlerTasks).ConfigureAwait(false);
    }

    #endregion
}
