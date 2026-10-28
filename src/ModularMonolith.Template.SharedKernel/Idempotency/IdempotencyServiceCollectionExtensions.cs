using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ModularMonolith.Template.SharedKernel.Idempotency;

public static class IdempotencyServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddIdempotency(Action<IdempotencyOptions>? configure = null)
        {
            if (configure is not null)
            {
                serviceCollection.Configure(configure);
            }

            serviceCollection.AddOptions<IdempotencyOptions>();
            serviceCollection.TryAddSingleton(TimeProvider.System);
            serviceCollection.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();
            return serviceCollection;
        }
    }
}
