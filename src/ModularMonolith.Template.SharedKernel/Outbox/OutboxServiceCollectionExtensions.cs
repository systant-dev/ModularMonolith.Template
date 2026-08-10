using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ModularMonolith.Template.SharedKernel.Outbox;

public static class OutboxServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers a module's <see cref="IOutboxStore"/> implementation so the outbox processor
        /// polls it. Use this for modules that own their persistence (e.g. Dapper).
        /// </summary>
        public IServiceCollection AddOutboxStore<TStore>()
            where TStore : class, IOutboxStore
        {
            serviceCollection.AddScoped<TStore>();
            serviceCollection.AddScoped<IOutboxStore>(provider => provider.GetRequiredService<TStore>());
            serviceCollection.GetOrAddOutboxStoreRegistry().Register(typeof(TStore));

            return serviceCollection;
        }

        public IServiceCollection AddOutboxProcessing(IConfiguration configuration)
        {
            serviceCollection.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));
            serviceCollection.GetOrAddOutboxStoreRegistry();
            serviceCollection.AddHostedService<OutboxProcessor>();

            return serviceCollection;
        }
    }

    #region Private Method Declarations

    private static OutboxStoreRegistry GetOrAddOutboxStoreRegistry(this IServiceCollection serviceCollection)
    {
        OutboxStoreRegistry? existing = serviceCollection
            .FirstOrDefault(descriptor => descriptor.ServiceType == typeof(OutboxStoreRegistry))?
            .ImplementationInstance as OutboxStoreRegistry;

        if (existing is not null)
        {
            return existing;
        }

        OutboxStoreRegistry registry = new();
        serviceCollection.TryAddSingleton(registry);
        return registry;
    }

    #endregion
}
