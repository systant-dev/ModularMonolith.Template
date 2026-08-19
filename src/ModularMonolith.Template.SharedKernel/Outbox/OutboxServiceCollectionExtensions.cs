using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModularMonolith.Template.SharedKernel.Messaging;
using System.Runtime.CompilerServices;

namespace ModularMonolith.Template.SharedKernel.Outbox;

public static class OutboxServiceCollectionExtensions
{
    #region Field Declarations

    private static readonly ConditionalWeakTable<IServiceCollection, OutboxStoreRegistry> Registries = new();

    #endregion

    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddOutboxStore<TStore>() where TStore : class, IOutboxStore
        {
            serviceCollection.AddScoped<TStore>();
            serviceCollection.AddScoped<IOutboxStore>(provider => provider.GetRequiredService<TStore>());
            serviceCollection.GetOrAddOutboxStoreRegistry().Register<TStore>();

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
        return Registries.GetValue(serviceCollection, static collection =>
        {
            ServiceDescriptor? descriptor = collection.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(OutboxStoreRegistry));

            if (descriptor is not null)
            {
                return descriptor.ImplementationInstance as OutboxStoreRegistry
                    ?? throw new InvalidOperationException(
                        $"{nameof(OutboxStoreRegistry)} is already registered with an incompatible implementation. " +
                        "Register outbox stores through AddOutboxStore/AddOutboxProcessing instead of registering the registry manually.");
            }

            OutboxStoreRegistry registry = new();
            collection.TryAddSingleton(registry);
            return registry;
        });
    }

    #endregion
}
