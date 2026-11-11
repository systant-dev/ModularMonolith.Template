using ModularMonolith.Template.SharedKernel.Configuration;

namespace UserModule.Infrastructure.Configuration;

internal sealed record UserModuleOptions : ModuleOptions
{
    #region Property Declarations

    public required int MaxUsers { get; init; }

    public required string ConnectionString { get; init; }

    #endregion
}
