namespace ModularMonolith.Template.SharedKernel.Messaging;

public interface IMessageDispatcher
{
    #region Method Declarations

    /// <summary>
    /// Invokes each handler one at a time, in registration order, awaiting each before starting the next.
    /// Prefer this when handlers share scoped, non-thread-safe dependencies (for example an EF Core DbContext).
    /// </summary>
    Task PublishSequentiallyAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : IMessage;

    /// <summary>
    /// Invokes all handlers concurrently and awaits their completion. Prefer this when handlers are independent
    /// and do not share non-thread-safe state. Note that every handler runs even if one fails.
    /// </summary>
    Task PublishConcurrentlyAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : IMessage;

    #endregion
}
