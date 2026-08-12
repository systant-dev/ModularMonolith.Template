using System.Collections.Concurrent;

namespace ModularMonolith.Template.SharedKernel.Outbox;

public sealed class OutboxStoreRegistry
{
    #region Field Declarations

    private readonly ConcurrentDictionary<Type, byte> _storeTypes = new();
    private volatile IReadOnlyCollection<Type> _snapshot = [];

    #endregion

    #region Property Declarations

    public IReadOnlyCollection<Type> StoreTypes => _snapshot;

    #endregion

    #region Public Method Declarations

    public void Register<TStore>() where TStore : class, IOutboxStore
    {
        Type storeType = typeof(TStore);

        // Rebuild the snapshot only when a new store type is added (registration is startup-only),
        // so the outbox processor's per-poll reads return a cached collection without allocating.
        if (_storeTypes.TryAdd(storeType, 0))
        {
            _snapshot = [.. _storeTypes.Keys];
        }
    }

    #endregion
}
