using System.Collections.Concurrent;
using System.Reflection;

namespace ModularMonolith.Template.SharedKernel.Messaging;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class MessageContractAttribute : Attribute
{
    #region Field Declarations

    private static readonly ConcurrentDictionary<Type, string> ContractNameCache = new();

    #endregion

    #region Property Declarations

    public string Name { get; }

    #endregion

    #region Constructor Declarations

    public MessageContractAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    #endregion

    #region Public Method Declarations

    public static string ResolveContractName(Type messageType)
    {
        ArgumentNullException.ThrowIfNull(messageType);

        return ContractNameCache.GetOrAdd(messageType, static type =>
        {
            MessageContractAttribute? attribute = type.GetCustomAttribute<MessageContractAttribute>(inherit: false);
            return attribute?.Name ?? type.FullName ?? type.Name;
        });
    }

    #endregion
}
