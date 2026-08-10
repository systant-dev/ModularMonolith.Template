using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ModularMonolith.Template.SharedKernel.Persistence;

namespace ModularMonolith.Template.SharedKernel.Outbox;

public static class OutboxServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddModuleDbContext<TContext>(Action<IServiceProvider, DbContextOptionsBuilder> configureOptions)
            where TContext : ModuleDbContext
        {
            ArgumentNullException.ThrowIfNull(configureOptions);

            serviceCollection.AddDbContext<TContext>(configureOptions);
            serviceCollection.GetOrAddOutboxRegistry().Register(typeof(TContext));

            return serviceCollection;
        }

        public IServiceCollection AddOutboxProcessing(IConfiguration configuration)
        {
            serviceCollection.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));
            serviceCollection.GetOrAddOutboxRegistry();
            serviceCollection.AddHostedService<OutboxProcessor>();

            return serviceCollection;
        }
    }

    #region Private Method Declarations

    private static OutboxContextRegistry GetOrAddOutboxRegistry(this IServiceCollection serviceCollection)
    {
        OutboxContextRegistry? existing = serviceCollection
            .FirstOrDefault(descriptor => descriptor.ServiceType == typeof(OutboxContextRegistry))?
            .ImplementationInstance as OutboxContextRegistry;

        if (existing is not null)
        {
            return existing;
        }

        OutboxContextRegistry registry = new();
        serviceCollection.TryAddSingleton(registry);
        return registry;
    }

    #endregion
}
