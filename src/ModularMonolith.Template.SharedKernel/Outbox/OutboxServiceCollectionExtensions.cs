using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ModularMonolith.Template.SharedKernel.Messaging;

namespace ModularMonolith.Template.SharedKernel.Outbox;

public static class OutboxServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
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
            serviceCollection
                .AddOptions<OutboxOptions>()
                .Bind(configuration.GetSection(OutboxOptions.SectionName))
                .ValidateDataAnnotations()
                .Validate(options => options.PollingInterval > TimeSpan.Zero, "Outbox.PollingInterval must be greater than zero.")
                .Validate(options => options.ClaimTimeout > TimeSpan.Zero, "Outbox.ClaimTimeout must be greater than zero.")
                .Validate(options => options.BackoffBaseDelay > TimeSpan.Zero, "Outbox.BackoffBaseDelay must be greater than zero.")
                .Validate(options => options.MaxBackoffDelay >= options.BackoffBaseDelay, "Outbox.MaxBackoffDelay must be greater than or equal to Outbox.BackoffBaseDelay.")
                .ValidateOnStart();

            serviceCollection.GetOrAddMessageContractRegistry();
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
