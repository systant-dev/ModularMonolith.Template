using System.Collections.Concurrent;

namespace ModularMonolith.Template.SharedKernel.Outbox;

public sealed class OutboxStoreRegistry
{
    #region Field Declarations

    private readonly ConcurrentDictionary<Type, byte> _storeTypes = new();

    #endregion

    #region Property Declarations

    public IReadOnlyCollection<Type> StoreTypes => [.. _storeTypes.Keys];

    #endregion

    #region Public Method Declarations

    public void Register<TStore>() where TStore : class, IOutboxStore
    {
        Type storeType = typeof(TStore);
        _storeTypes.TryAdd(storeType, 0);
    }

    #endregion
}
