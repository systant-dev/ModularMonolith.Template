using System.Collections.Concurrent;

namespace ModularMonolith.Template.SharedKernel.Idempotency;

internal sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    #region Field Declarations

    private readonly ConcurrentDictionary<string, byte> _reservedKeys = new(StringComparer.Ordinal);

    #endregion

    #region Public Method Declarations

    public ValueTask<bool> TryReserveAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        bool reserved = _reservedKeys.TryAdd(idempotencyKey, value: 0);
        return ValueTask.FromResult(reserved);
    }

    #endregion
}
