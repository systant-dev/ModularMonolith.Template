namespace ModularMonolith.Template.ModularGen.Tests.Helpers;

internal static class TestSources
{
    #region Field Declarations

    public const string MarkerTypes = """
        namespace Microsoft.Extensions.DependencyInjection
        {
            public interface IServiceCollection { }

            public interface IHealthChecksBuilder { }

            public static class HealthCheckServiceCollectionExtensions
            {
                public static IHealthChecksBuilder AddHealthChecks(this IServiceCollection services) => default!;
            }
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
                void RegisterHealthChecks(IHealthChecksBuilder healthChecks) { }
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

    public const string MessagingMarkerTypes = """
        namespace Microsoft.Extensions.DependencyInjection
        {
            public interface IServiceCollection { }
        }

        namespace Microsoft.Extensions.DependencyInjection.Extensions
        {
            using Microsoft.Extensions.DependencyInjection;

            public static class ServiceCollectionDescriptorExtensions
            {
                public static IServiceCollection TryAddSingleton<TService, TImplementation>(this IServiceCollection services)
                    where TImplementation : class, TService => services;
            }
        }

        namespace ModularMonolith.Template.SharedKernel.Messaging
        {
            public interface IMessage { }
        }

        namespace ModularMonolith.Template.SharedKernel.Messaging.Handling
        {
            using ModularMonolith.Template.SharedKernel.Messaging;

            public interface IMessageHandler<in TMessage>
                where TMessage : IMessage
            {
                System.Threading.Tasks.Task HandleAsync(TMessage message, System.Threading.CancellationToken cancellationToken = default);
            }
        }

        namespace ModularMonolith.Template.SharedKernel.Messaging.Dispatch
        {
            using ModularMonolith.Template.SharedKernel.Messaging;

            public enum DispatchMode { Sequential, Concurrent }

            public interface IMessageDispatchMap
            {
                System.Threading.Tasks.Task DispatchAsync(System.IServiceProvider serviceProvider, IMessage message, DispatchMode mode, System.Threading.CancellationToken cancellationToken);
                bool CanDispatch(System.Type messageType);
            }

        }

        namespace ModularMonolith.Template.SharedKernel.Messaging.Handling
        {
            using ModularMonolith.Template.SharedKernel.Messaging;
            using ModularMonolith.Template.SharedKernel.Messaging.Dispatch;

            public static class MessageHandlerInvoker
            {
                public static System.Threading.Tasks.Task InvokeAsync<TMessage>(System.IServiceProvider serviceProvider, TMessage message, DispatchMode mode, System.Threading.CancellationToken cancellationToken)
                    where TMessage : IMessage => System.Threading.Tasks.Task.CompletedTask;
            }
        }

        namespace ModularMonolith.Template.SharedKernel.Messaging.Dispatch
        {
            [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
            public sealed class MessageDispatchMapTargetAttribute : System.Attribute { }
        }
        """;

    public const string ExampleMessageAndHandler = """
        namespace ExampleModuleA.Messages
        {
            using ModularMonolith.Template.SharedKernel.Messaging;
            using ModularMonolith.Template.SharedKernel.Messaging.Handling;

            public sealed record UserCreated : IMessage;

            public sealed class UserCreatedHandler : IMessageHandler<UserCreated>
            {
                public System.Threading.Tasks.Task HandleAsync(UserCreated message, System.Threading.CancellationToken cancellationToken = default)
                    => System.Threading.Tasks.Task.CompletedTask;
            }
        }
        """;

    #endregion
}
