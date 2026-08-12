using System.Reflection;

namespace ModularMonolith.Template.SharedKernel.Messaging;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class MessageContractAttribute : Attribute
{
    #region Constructor Declarations

    public MessageContractAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    #endregion

    #region Property Declarations

    public string Name { get; }

    #endregion

    #region Public Method Declarations

    public static string ResolveContractName(Type messageType)
    {
        ArgumentNullException.ThrowIfNull(messageType);

        MessageContractAttribute? attribute = messageType.GetCustomAttribute<MessageContractAttribute>(inherit: false);
        return attribute?.Name ?? messageType.FullName ?? messageType.Name;
    }

    #endregion
}
