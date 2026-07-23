using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Template.SharedKernel.Modular;

public interface IModule
{
    #region Method Declarations

    void RegisterServices(IServiceCollection serviceCollection);

    void MapEndpoints();

    #endregion
}