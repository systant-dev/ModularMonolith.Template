using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace ModularMonolith.Template.SharedKernel.Idempotency;

internal sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    #region Field Declarations

    private const int SweepThreshold = 1024;

    private readonly ConcurrentDictionary<string, DateTimeOffset> _reservedKeys = new(StringComparer.Ordinal);
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _reservationDuration;

    #endregion

    #region Constructor Declarations

    public InMemoryIdempotencyStore(IOptions<IdempotencyOptions> options, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);

        _timeProvider = timeProvider ?? TimeProvider.System;
        _reservationDuration = options.Value.ReservationDuration;
    }

    #endregion

    #region Public Method Declarations

    public ValueTask<bool> TryReserveAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);

        DateTimeOffset now = _timeProvider.GetUtcNow();
        DateTimeOffset expiresOn = now + _reservationDuration;

        SweepIfRequired(now);

        while (true)
        {
            if (_reservedKeys.TryAdd(idempotencyKey, expiresOn))
            {
                return ValueTask.FromResult(true);
            }

            if (!_reservedKeys.TryGetValue(idempotencyKey, out DateTimeOffset existingExpiry))
            {
                continue;
            }

            if (existingExpiry > now)
            {
                return ValueTask.FromResult(false);
            }

            if (_reservedKeys.TryUpdate(idempotencyKey, expiresOn, existingExpiry))
            {
                return ValueTask.FromResult(true);
            }
        }
    }

    public ValueTask ReleaseAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);

        _reservedKeys.TryRemove(idempotencyKey, out _);
        return ValueTask.CompletedTask;
    }

    #endregion

    #region Private Method Declarations

    private void SweepIfRequired(DateTimeOffset now)
    {
        if (_reservedKeys.Count < SweepThreshold)
        {
            return;
        }

        foreach (KeyValuePair<string, DateTimeOffset> entry in _reservedKeys)
        {
            if (entry.Value <= now)
            {
                ((ICollection<KeyValuePair<string, DateTimeOffset>>)_reservedKeys).Remove(entry);
            }
        }
    }

    #endregion
}
