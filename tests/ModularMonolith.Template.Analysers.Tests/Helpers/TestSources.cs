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

    public const string CrossModuleTypes = """
        namespace UserModule
        {
            public sealed class UserStore
            {
                public int Count() => 0;
            }
        }

        namespace UserModule.Contracts
        {
            public sealed class UserResponse
            {
                public int Id { get; set; }
            }
        }
        """;

    public const string ModuleReferencingOtherModuleInternal = """
        namespace OrderModule
        {
            using UserModule;

            public sealed class OrderService
            {
                public int Count()
                {
                    UserStore store = new();
                    return store.Count();
                }
            }
        }
        """;

    public const string ModuleReferencingOtherModuleContracts = """
        namespace OrderModule
        {
            using UserModule.Contracts;

            public sealed class OrderService
            {
                public int UserId()
                {
                    UserResponse response = new();
                    return response.Id;
                }
            }
        }
        """;

    public const string ModuleReferencingOwnModuleInternal = """
        namespace UserModule
        {
            public sealed class UserService
            {
                public int Count()
                {
                    UserStore store = new();
                    return store.Count();
                }
            }
        }
        """;

    public const string NonModuleReferencingModuleInternal = """
        namespace Api
        {
            using UserModule;

            public sealed class Startup
            {
                public int Count()
                {
                    UserStore store = new();
                    return store.Count();
                }
            }
        }
        """;

    #endregion
}
