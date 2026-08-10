using ModularMonolith.Template.SharedKernel.Messaging;

namespace ModularMonolith.Template.SharedKernel.Outbox;

/// <summary>
/// Abstraction over a module's outbox storage. Implementations may be backed by
/// EF Core, Dapper, or any other persistence technology. Resolved per scope by the
/// <see cref="OutboxProcessor"/> so each module can own its transactional store.
/// </summary>
public interface IOutboxStore
{
    #region Method Declarations

    /// <summary>
    /// Enqueues a message to be dispatched by the outbox processor.
    /// </summary>
    Task EnqueueAsync(IMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the next batch of unprocessed messages that have not exhausted their attempts.
    /// </summary>
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxAttempts, int batchSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the updated processing state of a message (attempts, processed timestamp, error).
    /// </summary>
    Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    #endregion
}
