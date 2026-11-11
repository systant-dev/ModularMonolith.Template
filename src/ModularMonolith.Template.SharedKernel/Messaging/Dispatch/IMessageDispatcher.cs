namespace ModularMonolith.Template.SharedKernel.Messaging.Dispatch;

public interface IMessageDispatcher
{
    #region Method Declarations

    Task PublishAsync(IMessage message, DispatchMode mode = DispatchMode.Sequential, CancellationToken cancellationToken = default);

    #endregion
}
