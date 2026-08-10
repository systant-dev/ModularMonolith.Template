using System.Collections.Concurrent;

namespace ModularMonolith.Template.SharedKernel.Outbox;

/// <summary>
/// Tracks the <see cref="IOutboxStore"/> implementation types registered by modules so the
/// <see cref="OutboxProcessor"/> can resolve one store per scope and poll each module's outbox.
/// </summary>
public sealed class OutboxStoreRegistry
{
    #region Field Declarations

    private readonly ConcurrentDictionary<Type, byte> _storeTypes = new();

    #endregion

    #region Property Declarations

    public IReadOnlyCollection<Type> StoreTypes => [.. _storeTypes.Keys];

    #endregion

    #region Public Method Declarations

    public void Register(Type storeType)
    {
        ArgumentNullException.ThrowIfNull(storeType);
        _storeTypes.TryAdd(storeType, 0);
    }

    #endregion
}
