using ModularMonolith.Template.SharedKernel.Modular;

namespace ModularMonolith.Template.Api.Modular;

public static partial class ModuleEndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder MapModuleEndpoints(IConfiguration configuration)
        {
            MapEndpoints(endpoints, configuration);
            return endpoints;
        }
    }

    #region Private Method Declarations

    [ModuleEndpointMappingTarget]
    private static partial void MapEndpoints(IEndpointRouteBuilder endpoints, IConfiguration configuration);

    #endregion
}