namespace ModularMonolith.Template.SharedKernel.Messaging;

public interface IMessageDispatcher
{
    #region Method Declarations

    Task PublishSequentiallyAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : IMessage;

    Task PublishConcurrentlyAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : IMessage;

    #endregion
}