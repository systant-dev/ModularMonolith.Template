using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.Template.SharedKernel.Outbox;

public sealed class OutboxOptions
{
    #region Constant Declarations

    public const string SectionName = "Outbox";

    #endregion

    #region Property Declarations

    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(10);

    [Range(1, int.MaxValue)]
    public int BatchSize { get; set; } = 20;

    [Range(1, int.MaxValue)]
    public int MaxAttempts { get; set; } = 5;

    public TimeSpan ClaimTimeout { get; set; } = TimeSpan.FromMinutes(2);

    public TimeSpan BackoffBaseDelay { get; set; } = TimeSpan.FromSeconds(5);

    public TimeSpan MaxBackoffDelay { get; set; } = TimeSpan.FromMinutes(5);

    [Range(1, int.MaxValue)]
    public int MaxDegreeOfParallelism { get; set; } = 1;

    /// <summary>
    /// When <see langword="true"/>, each message's state is persisted immediately after it is
    /// dispatched (sequential processing only), narrowing the window in which a crash can cause a
    /// message to be re-dispatched. When <see langword="false"/> (the default), the whole batch is
    /// persisted once after all messages are dispatched, favouring throughput over a tighter
    /// duplicate window. Delivery remains at-least-once either way, so handlers must be idempotent.
    /// </summary>
    public bool PersistAfterEachMessage { get; set; }

    #endregion
}
