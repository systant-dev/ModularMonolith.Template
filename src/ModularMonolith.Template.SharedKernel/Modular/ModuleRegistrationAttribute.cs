namespace ModularMonolith.Template.SharedKernel.Modular;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class ModuleRegistrationAttribute<TModule> : Attribute where TModule : IModule
{
    #region Property Declarations

    public Type ModuleType => typeof(TModule);

    #endregion
}