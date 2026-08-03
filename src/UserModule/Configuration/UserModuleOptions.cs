using ModularMonolith.Template.SharedKernel.Configuration;

namespace UserModule.Configuration;

internal sealed record UserModuleOptions : ModuleOptions
{
    #region Property Declarations

    public int MaxUsers { get; init; } = 1000;

    #endregion
}
