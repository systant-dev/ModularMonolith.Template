namespace ModularMonolith.Template.SharedKernel.Messaging.Handling;

public interface IMessageHandler<in TMessage> where TMessage : IMessage
{
    #region Method Declarations

    Task HandleAsync(TMessage message, CancellationToken cancellationToken = default);

    #endregion
}
