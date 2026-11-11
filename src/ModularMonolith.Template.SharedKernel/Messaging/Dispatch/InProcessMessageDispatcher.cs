namespace ModularMonolith.Template.SharedKernel.Messaging.Dispatch;

internal sealed class InProcessMessageDispatcher(IServiceProvider serviceProvider, IMessageDispatchMap dispatchMap) : IMessageDispatcher
{
    #region Public Method Declarations

    public Task PublishAsync(IMessage message, DispatchMode mode = DispatchMode.Sequential, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        return dispatchMap.DispatchAsync(serviceProvider, message, mode, cancellationToken);
    }

    #endregion
}
