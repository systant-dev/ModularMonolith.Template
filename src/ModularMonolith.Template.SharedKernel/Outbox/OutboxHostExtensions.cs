using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Persistence;

namespace ModularMonolith.Template.SharedKernel.Outbox;

public static class OutboxHostExtensions
{
    extension(IServiceProvider serviceProvider)
    {
        public async Task MigrateModuleDatabasesAsync(CancellationToken cancellationToken = default)
        {
            await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();

            OutboxContextRegistry registry = scope.ServiceProvider.GetRequiredService<OutboxContextRegistry>();

            foreach (Type contextType in registry.ContextTypes)
            {
                if (scope.ServiceProvider.GetRequiredService(contextType) is ModuleDbContext context)
                {
                    // Relational providers (e.g. SQL Server) support migrations, while the
                    // in-memory provider used by examples/tests does not. Fall back to
                    // EnsureCreated for non-relational providers.
                    if (context.Database.IsRelational())
                    {
                        await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
