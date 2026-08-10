using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Template.SharedKernel.Outbox;

public static class OutboxHostExtensions
{
    extension(IServiceProvider serviceProvider)
    {
        public async Task MigrateModuleDatabasesAsync(CancellationToken cancellationToken = default)
        {
            await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();

            IEnumerable<IModuleDatabaseInitialiser> initialisers =
                scope.ServiceProvider.GetServices<IModuleDatabaseInitialiser>();

            foreach (IModuleDatabaseInitialiser initialiser in initialisers)
            {
                await initialiser.InitialiseAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
