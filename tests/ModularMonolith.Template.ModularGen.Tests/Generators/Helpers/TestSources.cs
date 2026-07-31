namespace ModularMonolith.Template.ModularGen.Tests.Generators.Helpers;

internal static class TestSources
{
    #region Field Declarations

    public const string MarkerTypes = """
        namespace Microsoft.Extensions.DependencyInjection
        {
            public interface IServiceCollection { }
        }

        namespace Microsoft.Extensions.Configuration
        {
            public interface IConfiguration { }

            public static class ConfigurationBinder
            {
                public static T GetValue<T>(this IConfiguration configuration, string key) => default!;
                public static T GetValue<T>(this IConfiguration configuration, string key, T defaultValue) => defaultValue;
            }
        }

        namespace Microsoft.AspNetCore.Routing
        {
            public interface IEndpointRouteBuilder { }
        }

        namespace Microsoft.AspNetCore.Builder
        {
            using Microsoft.AspNetCore.Routing;

            public static class EndpointRouteBuilderExtensions
            {
                public static IEndpointRouteBuilder MapGroup(this IEndpointRouteBuilder builder, string prefix) => builder;
            }
        }

        namespace Microsoft.AspNetCore.Http
        {
            using Microsoft.AspNetCore.Routing;

            public static class EndpointConventionExtensions
            {
                public static IEndpointRouteBuilder WithTags(this IEndpointRouteBuilder builder, params string[] tags) => builder;
            }
        }

        namespace ModularMonolith.Template.SharedKernel.Modular
        {
            using Microsoft.AspNetCore.Routing;
            using Microsoft.Extensions.DependencyInjection;

            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
            public sealed class ModuleAttribute : System.Attribute { }

            public interface IModule
            {
                void RegisterServices(IServiceCollection serviceCollection);
                void MapEndpoints(IEndpointRouteBuilder endpoints);
            }

            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class ModuleRegistrationAttribute<TModule> : System.Attribute
                where TModule : IModule { }

            [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
            public sealed class ModuleRegistrationTargetAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
            public sealed class ModuleEndpointMappingTargetAttribute : System.Attribute { }
        }
        """;

    public const string ExampleModule = """
        namespace ExampleModuleA
        {
            using Microsoft.AspNetCore.Routing;
            using Microsoft.Extensions.DependencyInjection;
            using ModularMonolith.Template.SharedKernel.Modular;

            [Module]
            public sealed class Module : IModule
            {
                public void RegisterServices(IServiceCollection serviceCollection) { }
                public void MapEndpoints(IEndpointRouteBuilder endpoints) { }
            }
        }
        """;

    #endregion
}