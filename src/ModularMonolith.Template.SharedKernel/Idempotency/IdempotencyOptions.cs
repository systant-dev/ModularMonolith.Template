namespace ModularMonolith.Template.SharedKernel.Idempotency;

public sealed class IdempotencyOptions
{
    #region Property Declarations

    public TimeSpan ReservationDuration { get; set; } = TimeSpan.FromHours(24);

    #endregion
}
