using AccountsModule.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;
using System.Data;

namespace AccountsModule.Storage;

internal sealed class SqlServerAccountStore(SqlServerConnectionFactory connectionFactory) : IAccountStore, IOutboxStore
{
    #region IAccountStore Method Declarations

    public async Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        IEnumerable<Account> accounts = await connection.QueryAsync<Account>(
            new CommandDefinition(
                "SELECT Id, UserId, AccountNumber, AccountHolderName, Balance FROM Accounts ORDER BY Id;",
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        return [.. accounts];
    }

    public async Task<Account?> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        return await connection.QuerySingleOrDefaultAsync<Account>(
            new CommandDefinition(
                "SELECT Id, UserId, AccountNumber, AccountHolderName, Balance FROM Accounts WHERE Id = @Id;",
                new { Id = id },
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);
    }

    public async Task<Account> CreateAsync(Account account, Func<Account, IMessage> integrationEventFactory, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using SqlTransaction transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        int id = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                """
                INSERT INTO Accounts (UserId, AccountNumber, AccountHolderName, Balance)
                OUTPUT INSERTED.Id
                VALUES (@UserId, @AccountNumber, @AccountHolderName, @Balance);
                """,
                new { account.UserId, account.AccountNumber, account.AccountHolderName, account.Balance },
                transaction,
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        Account created = account with { Id = id };

        await InsertOutboxMessageAsync(connection, transaction, OutboxMessage.Create(integrationEventFactory(created)), cancellationToken).ConfigureAwait(false);

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        return created;
    }

    public async Task<bool> UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        int affected = await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE Accounts
                SET AccountHolderName = @AccountHolderName, Balance = @Balance
                WHERE Id = @Id;
                """,
                new { account.Id, account.AccountHolderName, account.Balance },
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        int affected = await connection.ExecuteAsync(
            new CommandDefinition(
                "DELETE FROM Accounts WHERE Id = @Id;",
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
