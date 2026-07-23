using ModularMonolith.Template.SharedKernel.Modular;

namespace ModularMonolith.Template.Api.Modular;

public static partial class ModuleServiceCollectionExtensions
{
    public static IServiceCollection AddModules(this IServiceCollection serviceCollection)
    {
        GetModules(serviceCollection);
        return serviceCollection;
    }

    #region Private Method Declarations

    [ModuleRegistrationTarget]
    private static partial void GetModules(IServiceCollection serviceCollection);

    #endregion
}