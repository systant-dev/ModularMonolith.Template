using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Template.SharedKernel.Outbox;

public static class OutboxHostExtensions
{
    extension(IServiceProvider serviceProvider)
    {
        public async Task MigrateModuleDatabasesAsync(CancellationToken cancellationToken = default)
        {
            await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
            foreach (IModuleDatabaseInitialiser initialiser in scope.ServiceProvider.GetServices<IModuleDatabaseInitialiser>())
            {
                await initialiser.InitialiseAsync(cancellationToken);
            }
        }
    }
}
