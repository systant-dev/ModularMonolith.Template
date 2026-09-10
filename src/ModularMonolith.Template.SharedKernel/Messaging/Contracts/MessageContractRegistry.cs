using System.Collections.Concurrent;

namespace ModularMonolith.Template.SharedKernel.Messaging.Contracts;

public sealed class MessageContractRegistry : IMessageContractRegistry
{
    #region Field Declarations

    private readonly ConcurrentDictionary<string, Type> _typesByName = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<Type, string> _namesByType = new();

    #endregion

    #region Public Method Declarations

    public void Register<TMessage>() where TMessage : IMessage
    {
        Type messageType = typeof(TMessage);
        Register(messageType);
    }

    public string GetContractName<TMessage>() where TMessage : IMessage
    {
        Type messageType = typeof(TMessage);
        return _namesByType.GetOrAdd(messageType, type =>
        {
            string name = MessageContractAttribute.ResolveContractName(type);
            _typesByName[name] = type;
            return name;
        });
    }

    public bool TryResolve(string contractName, out Type messageType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contractName);

        if (_typesByName.TryGetValue(contractName, out Type? registered))
        {
            messageType = registered;
            return true;
        }

        messageType = null!;
        return false;
    }

    #endregion

    #region Private Method Declarations

    private void Register(Type messageType)
    {
        ArgumentNullException.ThrowIfNull(messageType);

        if (!typeof(IMessage).IsAssignableFrom(messageType))
        {
            return;
        }

        string name = MessageContractAttribute.ResolveContractName(messageType);
        _typesByName[name] = messageType;
        _namesByType[messageType] = name;
    }

    #endregion
}
