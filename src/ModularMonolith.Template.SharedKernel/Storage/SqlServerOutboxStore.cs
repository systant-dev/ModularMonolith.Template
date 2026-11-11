using Dapper;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;
using System.Data.Common;
using System.Text.Json;

namespace ModularMonolith.Template.SharedKernel.Storage;

public abstract class SqlServerOutboxStore(IDbConnectionFactory connectionFactory, IOutboxInterceptor outboxInterceptor) : IOutboxStore
{
    #region Public Method Declarations

    public async Task EnqueueAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await outboxInterceptor.InterceptAsync(connection, transaction: null, message, cancellationToken);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxAttempts, int batchSize, TimeSpan visibilityTimeout, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        int visibility = (int)Math.Ceiling(visibilityTimeout.TotalSeconds);
        var parameters = new { MaxAttempts = maxAttempts, BatchSize = batchSize, VisibilitySeconds = visibility };
        CommandDefinition command = new(
            """
            WITH claimable AS (
                SELECT TOP (@BatchSize) Id, Type, Payload, OccurredOnUtc,
                       ProcessedOnUtc, Attempts, Error, NextAttemptOnUtc
                FROM dbo.OutboxMessages WITH (READPAST, UPDLOCK)
                WHERE ProcessedOnUtc IS NULL
                  AND Attempts < @MaxAttempts
                  AND (NextAttemptOnUtc IS NULL OR NextAttemptOnUtc <= SYSUTCDATETIME())
                ORDER BY OccurredOnUtc
            )
            UPDATE claimable
            SET NextAttemptOnUtc = DATEADD(SECOND, @VisibilitySeconds, SYSUTCDATETIME())
            OUTPUT INSERTED.Id, INSERTED.Type, INSERTED.Payload, INSERTED.OccurredOnUtc,
                   INSERTED.ProcessedOnUtc, INSERTED.Attempts, INSERTED.Error, INSERTED.NextAttemptOnUtc;
            """,
            parameters,
            cancellationToken: cancellationToken);

        IEnumerable<OutboxMessage> messages = await connection.QueryAsync<OutboxMessage>(command);
        return messages as List<OutboxMessage> ?? [.. messages];
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetDeadLetteredAsync(int maxAttempts, int batchSize, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new { MaxAttempts = maxAttempts, BatchSize = batchSize };
        CommandDefinition command = new(
            """
            SELECT TOP (@BatchSize) Id, Type, Payload, OccurredOnUtc,
                   ProcessedOnUtc, Attempts, Error, NextAttemptOnUtc
            FROM dbo.OutboxMessages
            WHERE ProcessedOnUtc IS NULL
              AND Attempts >= @MaxAttempts
            ORDER BY OccurredOnUtc;
            """,
            parameters,
            cancellationToken: cancellationToken);

        IEnumerable<OutboxMessage> messages = await connection.QueryAsync<OutboxMessage>(command);
        return messages as List<OutboxMessage> ?? [.. messages];
    }

    public async Task UpdateAsync(IReadOnlyList<OutboxMessage> messages, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messages);
        if (messages.Count == 0)
        {
            return;
        }

        string json = JsonSerializer.Serialize(messages.Select(message => new
        {
            message.Id,
            message.ProcessedOnUtc,
            message.Attempts,
            message.Error,
            message.NextAttemptOnUtc
        }));

        await using DbConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        CommandDefinition command = new(
            """
            UPDATE outbox
            SET outbox.ProcessedOnUtc = source.ProcessedOnUtc,
                outbox.Attempts = source.Attempts,
                outbox.Error = source.Error,
                outbox.NextAttemptOnUtc = source.NextAttemptOnUtc
            FROM OutboxMessages AS outbox
            INNER JOIN OPENJSON(@Json)
            WITH (
                Id UNIQUEIDENTIFIER,
                ProcessedOnUtc DATETIMEOFFSET,
                Attempts INT,
                Error NVARCHAR(MAX),
                NextAttemptOnUtc DATETIMEOFFSET
            ) AS source ON source.Id = outbox.Id;
            """,
            new { Json = json },
            cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
    }

    public async Task<int> PurgeProcessedAsync(DateTimeOffset processedBefore, int batchSize, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new { ProcessedBefore = processedBefore, BatchSize = batchSize };
        CommandDefinition command = new(
            """
            DELETE TOP (@BatchSize) FROM dbo.OutboxMessages
            WHERE ProcessedOnUtc IS NOT NULL AND ProcessedOnUtc < @ProcessedBefore;
            """,
            parameters,
            cancellationToken: cancellationToken);

        return await connection.ExecuteAsync(command);
    }

    #endregion

}
