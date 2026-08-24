using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace ModularMonolith.Template.SharedKernel.Paging;

public static class PagingServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddPaging(IConfiguration? configuration = null)
        {
            OptionsBuilder<PagingOptions> optionsBuilder = serviceCollection.AddOptions<PagingOptions>();

            if (configuration is not null)
            {
                optionsBuilder.Bind(configuration.GetSection(PagingOptions.SectionName));
            }

            serviceCollection.TryAddSingleton<IPagingService, PagingService>();
            return serviceCollection;
        }
    }
}
