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
}
