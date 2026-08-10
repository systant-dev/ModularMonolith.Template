namespace ModularMonolith.Template.SharedKernel.Outbox;

public sealed class OutboxOptions
{
    #region Constant Declarations

    public const string SectionName = "Outbox";

    #endregion

    #region Property Declarations

    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(10);

    public int BatchSize { get; set; } = 20;

    public int MaxAttempts { get; set; } = 5;

    #endregion
}
