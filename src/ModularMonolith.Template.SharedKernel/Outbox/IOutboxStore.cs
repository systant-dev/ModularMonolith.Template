using ModularMonolith.Template.SharedKernel.Messaging;

namespace ModularMonolith.Template.SharedKernel.Outbox;

public interface IOutboxStore
{
    #region Method Declarations

    Task EnqueueAsync(IMessage message, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxAttempts, int batchSize, TimeSpan visibilityTimeout, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OutboxMessage>> GetDeadLetteredAsync(int maxAttempts, int batchSize, CancellationToken cancellationToken = default);

    Task UpdateAsync(IReadOnlyList<OutboxMessage> messages, CancellationToken cancellationToken = default);

    #endregion
}
