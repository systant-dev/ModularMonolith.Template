using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace ModularMonolith.Template.SharedKernel.Rules;

public static class RulesServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddRuleProcessors()
        {
            serviceCollection.TryAddScoped(typeof(IRuleProcessor<>), typeof(RuleProcessor<>));
            return serviceCollection;
        }

        public IServiceCollection AddRule<TContext, TRule>() where TRule : class, IRule<TContext>
        {
            serviceCollection.AddScoped<IRule<TContext>, TRule>();
            return serviceCollection;
        }

        public IServiceCollection AddRule<TContext>()
        {
            Type ruleContract = typeof(IRule<TContext>);
            IEnumerable<Type> moduleRules = GetLoadableTypes(Assembly.GetCallingAssembly())
                                                .Where(type => type is { IsClass: true, IsAbstract: false, ContainsGenericParameters: false })
                                                .Where(ruleContract.IsAssignableFrom);

            foreach (Type ruleType in moduleRules)
            {
                serviceCollection.AddScoped(ruleContract, ruleType);
            }

            return serviceCollection;
        }
    }

    #region Private Method Declarations

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.OfType<Type>();
        }
    }

    #endregion
}
