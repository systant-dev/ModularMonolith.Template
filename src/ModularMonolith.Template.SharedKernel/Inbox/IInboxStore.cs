namespace ModularMonolith.Template.SharedKernel.Inbox;

public interface IInboxStore
{
    #region Method Declarations

    Task<bool> IsProcessedAsync(string consumer, string messageId, CancellationToken cancellationToken = default);

    Task<bool> TryMarkProcessedAsync(string consumer, string messageId, CancellationToken cancellationToken = default);

    #endregion
}
