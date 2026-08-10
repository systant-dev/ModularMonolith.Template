namespace ModularMonolith.Template.SharedKernel.Messaging;

public interface IMessageContractRegistry
{
    #region Method Declarations

    void Register(Type messageType);

    string GetContractName(Type messageType);

    bool TryResolve(string contractName, out Type messageType);

    #endregion
}
