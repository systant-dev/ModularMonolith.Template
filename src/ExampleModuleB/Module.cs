using ExampleModuleB.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Configuration;
using ModularMonolith.Template.SharedKernel.Modular;

namespace ExampleModuleB;

[Module]
public sealed class Module : IModule
{
    #region Public Method Declarations

    ///<inheritdoc/>
    public void RegisterServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddValidatedOptions<ExampleModuleBOptions, ExampleModuleBOptionsValidator>(nameof(ExampleModuleB));
    }

    ///<inheritdoc/>
    public void MapEndpoints()
    {
        throw new NotImplementedException();
    }

    #endregion  
}