namespace ModularMonolith.Template.SharedKernel.Configuration;

public abstract record ModuleOptions : IModuleOptions
{
    #region Property Declarations

    public bool Enabled { get; init; } = true;

    #endregion
}
