using System.Runtime.CompilerServices;

namespace ModularMonolith.Template.ComplianceGen.Tests.Helpers;

internal static class ModuleInitializer
{
    #region Public Method Declarations

    [ModuleInitializer]
    public static void Initialize()
    {
        VerifySourceGenerators.Initialize();
        UseProjectRelativeDirectory("Snapshots");
    }

    #endregion
}
