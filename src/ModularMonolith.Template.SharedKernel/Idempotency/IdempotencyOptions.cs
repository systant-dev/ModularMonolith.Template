namespace ModularMonolith.Template.SharedKernel.Idempotency;

public sealed class IdempotencyOptions
{
    #region Property Declarations

    /// <summary>
    /// How long a reserved idempotency key is retained before it expires and can be reused.
    /// Bounds the memory used by the in-memory store and allows a genuine retry after the window elapses.
    /// </summary>
    public TimeSpan ReservationDuration { get; set; } = TimeSpan.FromHours(24);

    #endregion
}
