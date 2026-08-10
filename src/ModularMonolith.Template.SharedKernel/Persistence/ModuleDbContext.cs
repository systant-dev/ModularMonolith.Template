using Microsoft.EntityFrameworkCore;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;

namespace ModularMonolith.Template.SharedKernel.Persistence;

public abstract class ModuleDbContext(DbContextOptions options) : DbContext(options)
{
    #region Property Declarations

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected abstract string Schema { get; }

    #endregion

    #region Public Method Declarations

    public void EnqueueOutboxMessage(IMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        OutboxMessages.Add(OutboxMessage.Create(message));
    }

    #endregion

    #region Protected Method Declarations

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

        base.OnModelCreating(modelBuilder);
    }

    #endregion
}
