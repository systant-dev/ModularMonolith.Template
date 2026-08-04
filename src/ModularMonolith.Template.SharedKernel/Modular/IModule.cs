using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ModularMonolith.Template.SharedKernel.Modular;

public interface IModule
{
    #region Method Declarations

    void RegisterServices(IServiceCollection serviceCollection);

    void MapEndpoints(IEndpointRouteBuilder endpoints);

    void RegisterHealthChecks(IHealthChecksBuilder healthChecks)
    {
    }

    #endregion
}