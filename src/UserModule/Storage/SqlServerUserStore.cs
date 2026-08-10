using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;
using UserModule.Models;

namespace UserModule.Storage;

internal sealed class SqlServerUserStore(SqlServerConnectionFactory connectionFactory) : IUserStore, IOutboxStore
{
    #region IUserStore Method Declarations

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        IEnumerable<User> users = await connection.QueryAsync<User>(
            new CommandDefinition(
                "SELECT Id, FullName, Email, PhoneNumber FROM Users ORDER BY Id;",
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        return [.. users];
    }

    public async Task<User?> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        return await connection.QuerySingleOrDefaultAsync<User>(
            new CommandDefinition(
                "SELECT Id, FullName, Email, PhoneNumber FROM Users WHERE Id = @Id;",
                new { Id = id },
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);
    }

    public async Task<User> CreateAsync(User user, Func<User, IMessage> integrationEventFactory, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using SqlTransaction transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        int id = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                """
                INSERT INTO Users (FullName, Email, PhoneNumber)
                OUTPUT INSERTED.Id
                VALUES (@FullName, @Email, @PhoneNumber);
                """,
                new { user.FullName, user.Email, user.PhoneNumber },
                transaction,
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        User created = user with { Id = id };

        await InsertOutboxMessageAsync(connection, transaction, OutboxMessage.Create(integrationEventFactory(created)), cancellationToken).ConfigureAwait(false);

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        return created;
    }

    public async Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        int affected = await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE Users
                SET FullName = @FullName, Email = @Email, PhoneNumber = @PhoneNumber
                WHERE Id = @Id;
                """,
                new { user.Id, user.FullName, user.Email, user.PhoneNumber },
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        int affected = await connection.ExecuteAsync(
            new CommandDefinition(
                "DELETE FROM Users WHERE Id = @Id;",
                new { Id = id },
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        return affected > 0;
    }

    #endregion

    #region IOutboxStore Method Declarations

    public async Task EnqueueAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await InsertOutboxMessageAsync(connection, transaction: null, OutboxMessage.Create(message), cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxAttempts, int batchSize, TimeSpan visibilityTimeout, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        // Atomically claim a batch: READPAST skips rows another processor already locked, and deferring
        // NextAttemptOnUtc by the visibility timeout leases the rows so competing processors won't
        // redeliver them until this attempt completes or the lease expires (crash recovery).
        IEnumerable<OutboxMessage> messages = await connection.QueryAsync<OutboxMessage>(
            new CommandDefinition(
                """
                UPDATE TOP (@BatchSize) dbo.OutboxMessages WITH (READPAST, UPDLOCK)
                SET NextAttemptOnUtc = DATEADD(SECOND, @VisibilitySeconds, SYSUTCDATETIME())
                OUTPUT INSERTED.Id, INSERTED.Type, INSERTED.Payload, INSERTED.OccurredOnUtc,
                       INSERTED.ProcessedOnUtc, INSERTED.Attempts, INSERTED.Error, INSERTED.NextAttemptOnUtc
                WHERE ProcessedOnUtc IS NULL
                  AND Attempts < @MaxAttempts
                  AND (NextAttemptOnUtc IS NULL OR NextAttemptOnUtc <= SYSUTCDATETIME());
                """,
                new
                {
                    MaxAttempts = maxAttempts,
                    BatchSize = batchSize,
                    VisibilitySeconds = (int)Math.Ceiling(visibilityTimeout.TotalSeconds)
                },
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        return [.. messages];
    }

    public async Task UpdateAsync(IReadOnlyList<OutboxMessage> messages, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messages);

        if (messages.Count == 0)
        {
            return;
        }

        await using SqlConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);

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

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE OutboxMessages
                SET ProcessedOnUtc = @ProcessedOnUtc, Attempts = @Attempts, Error = @Error, NextAttemptOnUtc = @NextAttemptOnUtc
                WHERE Id = @Id;
                """,
                parameters,
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);
    }

    #endregion

    #region Private Method Declarations

    private static async Task InsertOutboxMessageAsync(
        IDbConnection connection,
        IDbTransaction? transaction,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                INSERT INTO OutboxMessages (Id, Type, Payload, OccurredOnUtc, ProcessedOnUtc, Attempts, Error, NextAttemptOnUtc)
                VALUES (@Id, @Type, @Payload, @OccurredOnUtc, @ProcessedOnUtc, @Attempts, @Error, @NextAttemptOnUtc);
                """,
                new
                {
                    message.Id,
                    message.Type,
                    message.Payload,
                    message.OccurredOnUtc,
                    message.ProcessedOnUtc,
                    message.Attempts,
                    message.Error,
                    message.NextAttemptOnUtc
                },
                transaction,
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);
    }

    #endregion
}
