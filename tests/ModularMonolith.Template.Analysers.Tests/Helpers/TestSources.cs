namespace ModularMonolith.Template.Analysers.Tests.Helpers;

internal static class TestSources
{
    #region Field Declarations

    public const string MarkerTypes = """
        namespace ModularMonolith.Template.SharedKernel.Modular
        {
            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
            public sealed class ModuleAttribute : System.Attribute { }

            public interface IModule { }
        }
        """;

    public const string ModuleImplementingInterface = """
        namespace Sample
        {
            using ModularMonolith.Template.SharedKernel.Modular;

            [Module]
            public sealed class GoodModule : IModule { }
        }
        """;

    public const string ModuleMissingInterface = """
        namespace Sample
        {
            using ModularMonolith.Template.SharedKernel.Modular;

            [Module]
            public sealed class BadModule { }
        }
        """;

    public const string TypeWithoutModuleAttribute = """
        namespace Sample
        {
            using ModularMonolith.Template.SharedKernel.Modular;

            public sealed class PlainType { }
        }
        """;

    public const string ModuleInheritingInterfaceFromBase = """
        namespace Sample
        {
            using ModularMonolith.Template.SharedKernel.Modular;

            public abstract class ModuleBase : IModule { }

            [Module]
            public sealed class DerivedModule : ModuleBase { }
        }
        """;

    #endregion
}
