namespace ModularMonolith.Template.SharedKernel.Messaging;

public interface IMessageDispatchMap
{
    #region Method Declarations

    Task DispatchAsync(IServiceProvider serviceProvider, IMessage message, DispatchMode mode, CancellationToken cancellationToken);

    bool CanDispatch(Type messageType);

    bool CanDispatch<TMessage>() where TMessage : IMessage => CanDispatch(typeof(TMessage));

    #endregion
}
