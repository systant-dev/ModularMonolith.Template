using ModularMonolith.Template.SharedKernel.Modular;

namespace ModularMonolith.Template.Api.Modular;

public static partial class ModuleServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddModules(IConfiguration configuration)
        {
            GetModules(serviceCollection, configuration);
            return serviceCollection;
        }
    }

    #region Private Method Declarations

    [ModuleRegistrationTarget]
    private static partial void GetModules(IServiceCollection serviceCollection, IConfiguration configuration);

    #endregion
}
