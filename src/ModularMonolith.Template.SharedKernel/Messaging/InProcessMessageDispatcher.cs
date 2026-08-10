using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;

namespace ModularMonolith.Template.SharedKernel.Messaging;

internal sealed class InProcessMessageDispatcher(IServiceProvider serviceProvider) : IMessageDispatcher
{
    #region Field Declarations

    private static readonly ConcurrentDictionary<Type, MethodInfo> PublishMethodCache = new();

    #endregion

    #region Public Method Declarations

    public async Task PublishSequentiallyAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        IEnumerable<IMessageHandler<TMessage>> handlers = serviceProvider.GetServices<IMessageHandler<TMessage>>();

        foreach (IMessageHandler<TMessage> handler in handlers)
        {
            await handler.HandleAsync(message, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task PublishConcurrentlyAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        IEnumerable<IMessageHandler<TMessage>> handlers = serviceProvider.GetServices<IMessageHandler<TMessage>>();

        IEnumerable<Task> handlerTasks = handlers.Select(handler => handler.HandleAsync(message, cancellationToken));

        await Task.WhenAll(handlerTasks).ConfigureAwait(false);
    }

    public Task PublishAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        MethodInfo method = PublishMethodCache.GetOrAdd(message.GetType(), static messageType =>
            typeof(InProcessMessageDispatcher)
                .GetMethod(nameof(PublishSequentiallyAsync))!
                .MakeGenericMethod(messageType));

        return (Task)method.Invoke(this, [message, cancellationToken])!;
    }

    #endregion
}
