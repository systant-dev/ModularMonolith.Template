using ExampleModuleA.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Modular;

namespace ExampleModuleA;

[Module]
public sealed class Module : IModule
{
    #region Public Method Declarations

    ///<inheritdoc/>
    public void RegisterServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<ExampleModuleAOptions>();
    }

    ///<inheritdoc/>
    public void MapEndpoints()
    {
        throw new NotImplementedException();
    }

    #endregion  
}