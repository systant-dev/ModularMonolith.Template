using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Compliance.Redaction;
using System.Reflection;

namespace ModularMonolith.Template.SharedKernel.Compliance;

public static class ComplianceServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddCompliance(Action<ComplianceOptions>? configure = null, params Assembly[] assemblies)
        {
            ComplianceOptions options = new();
            configure?.Invoke(options);

            serviceCollection.AddSingleton(options);
            serviceCollection.AddSingleton<IRedactorProvider, RedactorProvider>();

            Assembly[] scanTargets = assemblies is { Length: > 0 } ? assemblies : [Assembly.GetCallingAssembly()];

            foreach (Assembly assembly in scanTargets)
            {
                RegisterModelRedactors(serviceCollection, assembly);
            }

            return serviceCollection;
        }
    }

    #region Private Method Declarations

    private static void RegisterModelRedactors(IServiceCollection serviceCollection, Assembly assembly)
    {
        foreach (Type type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
            {
                continue;
            }

            foreach (Type @interface in type.GetInterfaces())
            {
                if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IModelRedactor<>))
                {
                    serviceCollection.AddSingleton(@interface, type);
                }
            }
        }
    }

    #endregion
}
