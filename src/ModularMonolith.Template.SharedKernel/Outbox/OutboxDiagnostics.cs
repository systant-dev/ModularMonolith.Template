using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ModularMonolith.Template.SharedKernel.Outbox;

public static class OutboxDiagnostics
{
    #region Constant Declarations

    public const string Name = "ModularMonolith.Template.Outbox";

    #endregion

    #region Field Declarations

    private static readonly Meter Meter = new(Name);

    #endregion

    #region Property Declarations

    public static ActivitySource ActivitySource { get; } = new(Name);

    public static Counter<long> ProcessedCounter { get; } =
        Meter.CreateCounter<long>("outbox.messages.processed", unit: "{message}", description: "Outbox messages dispatched successfully.");

    public static Counter<long> FailedCounter { get; } =
        Meter.CreateCounter<long>("outbox.messages.failed", unit: "{message}", description: "Outbox message dispatch attempts that failed.");

    public static Counter<long> RetriedCounter { get; } =
        Meter.CreateCounter<long>("outbox.messages.retried", unit: "{message}", description: "Outbox messages scheduled for a retry after a failed attempt.");

    public static Counter<long> DeadLetteredCounter { get; } =
        Meter.CreateCounter<long>("outbox.messages.dead_lettered", unit: "{message}", description: "Outbox messages that exhausted their retry budget.");

    #endregion
}
