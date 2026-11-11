using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Storage;

namespace ModularMonolith.Template.SharedKernel.Inbox;

public static class InboxServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddSqlServerInboxStore(string connectionFactoryKey)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionFactoryKey);

            serviceCollection.AddKeyedScoped<IInboxStore>(connectionFactoryKey, static (provider, key) => new SqlServerInboxStore(provider.GetRequiredKeyedService<IDbConnectionFactory>(key)));
            return serviceCollection;
        }
    }
}
