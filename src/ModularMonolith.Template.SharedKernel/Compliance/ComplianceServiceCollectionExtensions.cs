using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ModularMonolith.Template.SharedKernel.Compliance;

/// <summary>
/// Registration helpers for the compliance redaction pipeline.
/// </summary>
public static class ComplianceServiceCollectionExtensions
{
    #region Public Method Declarations

    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the compliance redaction pipeline: a configured <see cref="IRedactorProvider"/>
        /// and every generated <see cref="IModelRedactor{T}"/> discovered in the supplied assemblies.
        /// </summary>
        /// <param name="configure">Optional callback to map classifications to redactors.</param>
        /// <param name="assemblies">
        /// Assemblies to scan for generated redactors. Defaults to the calling assembly when none are supplied.
        /// </param>
        public IServiceCollection AddCompliance(Action<ComplianceOptions>? configure = null, params Assembly[] assemblies)
        {
            ComplianceOptions options = new();
            configure?.Invoke(options);

            serviceCollection.AddSingleton(options);
            serviceCollection.AddSingleton<IRedactorProvider, RedactorProvider>();

            Assembly[] scanTargets = assemblies is { Length: > 0 }
                ? assemblies
                : [Assembly.GetCallingAssembly()];

            foreach (Assembly assembly in scanTargets)
            {
                RegisterModelRedactors(serviceCollection, assembly);
            }

            return serviceCollection;
        }
    }

    #endregion

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
