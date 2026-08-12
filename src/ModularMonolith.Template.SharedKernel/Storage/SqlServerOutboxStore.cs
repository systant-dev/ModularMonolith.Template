using Dapper;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;
using System.Data;
using System.Data.Common;
using System.Text.Json;

namespace ModularMonolith.Template.SharedKernel.Storage;

public abstract class SqlServerOutboxStore(IDbConnectionFactory connectionFactory) : IOutboxStore
{
    #region Protected Property Declarations

    protected IDbConnectionFactory ConnectionFactory => connectionFactory;

    #endregion

    #region IOutboxStore Method Declarations

    public async Task EnqueueAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await InsertOutboxMessageAsync(connection, transaction: null, OutboxMessage.Create(message), cancellationToken);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxAttempts, int batchSize, TimeSpan visibilityTimeout, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        // Atomically claim a batch: READPAST skips rows another processor already locked, and deferring
        // NextAttemptOnUtc by the visibility timeout leases the rows so competing processors won't
        // redeliver them until this attempt completes or the lease expires (crash recovery).
        var parameters = new
        {
            MaxAttempts = maxAttempts,
            BatchSize = batchSize,
            VisibilitySeconds = (int)Math.Ceiling(visibilityTimeout.TotalSeconds)
        };
        CommandDefinition command = new(
            """
            UPDATE TOP (@BatchSize) dbo.OutboxMessages WITH (READPAST, UPDLOCK)
            SET NextAttemptOnUtc = DATEADD(SECOND, @VisibilitySeconds, SYSUTCDATETIME())
            OUTPUT INSERTED.Id, INSERTED.Type, INSERTED.Payload, INSERTED.OccurredOnUtc,
                   INSERTED.ProcessedOnUtc, INSERTED.Attempts, INSERTED.Error, INSERTED.NextAttemptOnUtc
            WHERE ProcessedOnUtc IS NULL
              AND Attempts < @MaxAttempts
              AND (NextAttemptOnUtc IS NULL OR NextAttemptOnUtc <= SYSUTCDATETIME());
            """,
            parameters,
            cancellationToken: cancellationToken);

        IEnumerable<OutboxMessage> messages = await connection.QueryAsync<OutboxMessage>(command);
        return [.. messages];
    }

    public async Task UpdateAsync(IReadOnlyList<OutboxMessage> messages, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messages);
        if (messages.Count == 0)
        {
            return;
        }

        // Persist the whole batch in a single set-based round-trip instead of one UPDATE per row.
        // The state is shipped as JSON and shredded with OPENJSON, then joined back to the table.
        // A transaction wraps the write so the batch state is applied atomically.
        string json = JsonSerializer.Serialize(messages.Select(message => new
        {
            message.Id,
            message.ProcessedOnUtc,
            message.Attempts,
            message.Error,
            message.NextAttemptOnUtc
        }));

        await using DbConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using DbTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

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
            transaction,
            cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
        await transaction.CommitAsync(cancellationToken);
    }

    #endregion

    #region Protected Method Declarations

    protected static async Task InsertOutboxMessageAsync(
        IDbConnection connection,
        IDbTransaction? transaction,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var parameters = new
        {
            message.Id,
            message.Type,
            message.Payload,
            message.OccurredOnUtc,
            message.ProcessedOnUtc,
            message.Attempts,
            message.Error,
            message.NextAttemptOnUtc
        };
        CommandDefinition command = new(
            """
            INSERT INTO OutboxMessages (Id, Type, Payload, OccurredOnUtc, ProcessedOnUtc, Attempts, Error, NextAttemptOnUtc)
            VALUES (@Id, @Type, @Payload, @OccurredOnUtc, @ProcessedOnUtc, @Attempts, @Error, @NextAttemptOnUtc);
            """,
            parameters,
            transaction,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    #endregion
}
