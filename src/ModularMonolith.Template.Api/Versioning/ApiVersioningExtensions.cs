using Asp.Versioning;

namespace ModularMonolith.Template.Api.Versioning;

public static class ApiVersioningExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddModuleApiVersioning()
        {
            serviceCollection.AddApiVersioning(options =>
                {
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ReportApiVersions = true;
                    options.ApiVersionReader = new UrlSegmentApiVersionReader();
                })
                .AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                });

            return serviceCollection;
        }
    }
}