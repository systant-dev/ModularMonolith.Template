using Dapper;
using ModularMonolith.Template.SharedKernel.Inbox;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;
using System.Data;
using System.Data.Common;
using System.Text.Json;

namespace ModularMonolith.Template.SharedKernel.Storage;

public abstract class SqlServerOutboxStore(IDbConnectionFactory connectionFactory,
                                           IOutboxInterceptor outboxInterceptor) : IOutboxStore, IInboxStore
{
    #region Property Declarations

    protected IDbConnectionFactory ConnectionFactory => connectionFactory;

    protected IOutboxInterceptor OutboxInterceptor => outboxInterceptor;

    #endregion

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
            DECLARE @OutboxPurged INT = 0;
            DECLARE @InboxPurged INT = 0;

            DELETE TOP (@BatchSize) FROM dbo.OutboxMessages
            WHERE ProcessedOnUtc IS NOT NULL AND ProcessedOnUtc < @ProcessedBefore;
            SET @OutboxPurged = @@ROWCOUNT;

            IF OBJECT_ID(N'dbo.InboxMessages', N'U') IS NOT NULL
            BEGIN
                DELETE TOP (@BatchSize) FROM dbo.InboxMessages
                WHERE ProcessedOnUtc < @ProcessedBefore;
                SET @InboxPurged = @@ROWCOUNT;
            END;

            SELECT CASE WHEN @OutboxPurged > @InboxPurged THEN @OutboxPurged ELSE @InboxPurged END;
            """,
            parameters,
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<int>(command);
    }

    public async Task<bool> IsProcessedAsync(string consumer, string messageId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consumer);
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);

        await using DbConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new { Consumer = consumer, MessageId = messageId };
        CommandDefinition command = new(
            """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.InboxMessages
                WHERE Consumer = @Consumer AND MessageId = @MessageId) THEN 1 ELSE 0 END;
            """,
            parameters,
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<bool> TryMarkProcessedAsync(string consumer, string messageId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consumer);
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);

        await using DbConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new { Consumer = consumer, MessageId = messageId };
        CommandDefinition command = new(
            """
            INSERT INTO dbo.InboxMessages (Consumer, MessageId)
            SELECT @Consumer, @MessageId
            WHERE NOT EXISTS (
                SELECT 1 FROM dbo.InboxMessages WITH (UPDLOCK, HOLDLOCK)
                WHERE Consumer = @Consumer AND MessageId = @MessageId);
            """,
            parameters,
            cancellationToken: cancellationToken);

        int inserted = await connection.ExecuteAsync(command);
        return inserted > 0;
    }

    #endregion

}
