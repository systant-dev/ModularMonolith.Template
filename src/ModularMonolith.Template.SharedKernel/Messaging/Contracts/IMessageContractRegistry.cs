namespace ModularMonolith.Template.SharedKernel.Messaging.Contracts;

public interface IMessageContractRegistry
{
    #region Method Declarations

    void Register<TMessage>() where TMessage : IMessage;

    string GetContractName<TMessage>() where TMessage : IMessage;

    bool TryResolve(string contractName, out Type messageType);

    #endregion
}
