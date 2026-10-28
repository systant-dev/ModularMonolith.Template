namespace ModularMonolith.Template.SharedKernel.Inbox;

public interface IInboxStore
{
    #region Method Declarations

    Task<bool> TryMarkProcessedAsync(string consumer, string messageId, CancellationToken cancellationToken = default);

    #endregion
}
