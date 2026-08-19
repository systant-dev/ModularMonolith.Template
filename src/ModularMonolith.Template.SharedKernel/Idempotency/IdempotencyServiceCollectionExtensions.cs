using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Template.SharedKernel.Idempotency;

public static class IdempotencyServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddIdempotency()
        {
            serviceCollection.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();
            return serviceCollection;
        }
    }
}
