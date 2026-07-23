namespace ModularMonolith.Template.SharedKernel.Configuration;

public abstract record ModuleOptions : IModuleOptions
{
    #region Property Declarations

    public required bool Enabled { get; init; }

    #endregion

    #region Constructor Declarations

    protected ModuleOptions()
    {
    }

    #endregion
}