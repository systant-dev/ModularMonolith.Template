namespace ModularMonolith.Template.SharedKernel.Idempotency;

public interface IIdempotencyStore
{
    #region Method Declarations

    ValueTask<bool> TryReserveAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    ValueTask ReleaseAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    #endregion
}
