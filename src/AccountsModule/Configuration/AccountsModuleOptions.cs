using ModularMonolith.Template.SharedKernel.Configuration;

namespace AccountsModule.Configuration;

internal sealed record AccountsModuleOptions : ModuleOptions
{
    #region Property Declarations

    public required int MaxAccounts { get; init; }

    public required string ConnectionString { get; init; }

    #endregion
}
