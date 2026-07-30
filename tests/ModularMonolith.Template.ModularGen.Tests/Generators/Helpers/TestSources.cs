namespace ModularMonolith.Template.ModularGen.Tests.Generators.Helpers;

internal static class TestSources
{
    public const string MarkerTypes = """
        namespace Microsoft.Extensions.DependencyInjection
        {
            public interface IServiceCollection { }
        }

        namespace ModularMonolith.Template.SharedKernel.Modular
        {
            using Microsoft.Extensions.DependencyInjection;

            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
            public sealed class ModuleAttribute : System.Attribute { }

            public interface IModule
            {
                void RegisterServices(IServiceCollection serviceCollection);
                void MapEndpoints();
            }

            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class ModuleRegistrationAttribute<TModule> : System.Attribute
                where TModule : IModule { }

            [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
            public sealed class ModuleRegistrationTargetAttribute : System.Attribute { }
        }
        """;

    public const string ExampleModule = """
        namespace ExampleModuleA
        {
            using Microsoft.Extensions.DependencyInjection;
            using ModularMonolith.Template.SharedKernel.Modular;

            [Module]
            public sealed class Module : IModule
            {
                public void RegisterServices(IServiceCollection serviceCollection) { }
                public void MapEndpoints() { }
            }
        }
        """;
}