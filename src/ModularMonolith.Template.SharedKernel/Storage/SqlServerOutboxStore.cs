using Dapper;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;
using System.Data;
using System.Data.Common;

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

        await using DbConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        object[] parameters =
        [
            .. messages.Select(message => new
            {
                message.Id,
                message.ProcessedOnUtc,
                message.Attempts,
                message.Error,
                message.NextAttemptOnUtc
            })
        ];

        CommandDefinition command = new(
            """
            UPDATE OutboxMessages
            SET ProcessedOnUtc = @ProcessedOnUtc, Attempts = @Attempts, Error = @Error, NextAttemptOnUtc = @NextAttemptOnUtc
            WHERE Id = @Id;
            """,
            parameters,
            cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
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
