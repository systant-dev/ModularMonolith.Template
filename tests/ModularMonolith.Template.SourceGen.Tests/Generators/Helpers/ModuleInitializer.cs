using System.Runtime.CompilerServices;

namespace ModularMonolith.Template.SourceGen.Tests.Generators.Helpers;

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