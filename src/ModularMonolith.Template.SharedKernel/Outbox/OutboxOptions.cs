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

    #endregion
}
