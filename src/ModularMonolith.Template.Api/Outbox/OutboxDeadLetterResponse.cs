namespace ModularMonolith.Template.Api.Outbox;

public sealed record OutboxDeadLetterResponse(Guid Id,
                                              string Type,
                                              DateTimeOffset OccurredOnUtc,
                                              int Attempts,
                                              string? Error);
