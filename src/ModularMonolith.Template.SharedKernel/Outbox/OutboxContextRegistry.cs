using System.Collections.Concurrent;

namespace ModularMonolith.Template.SharedKernel.Outbox;

public sealed class OutboxContextRegistry
{
    #region Field Declarations

    private readonly ConcurrentDictionary<Type, byte> _contextTypes = new();

    #endregion

    #region Property Declarations

    public IReadOnlyCollection<Type> ContextTypes => [.. _contextTypes.Keys];

    #endregion

    #region Public Method Declarations

    public void Register(Type contextType)
    {
        ArgumentNullException.ThrowIfNull(contextType);
        _contextTypes.TryAdd(contextType, 0);
    }

    #endregion
}
