using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;
using UserModule.Models;

namespace UserModule.Storage;

/// <summary>
/// Dapper based data access for the UserModule, backed by SQL Server. Implements both
/// <see cref="IUserStore"/> and <see cref="IOutboxStore"/> so that <see cref="CreateAsync"/> can
/// insert the user and enqueue the resulting integration event inside a single SQL Server
/// transaction. The SQL uses SQL Server dialect (<c>OUTPUT INSERTED.Id</c>, <c>OFFSET/FETCH</c>,
/// <c>UPDLOCK, READPAST</c>) so it runs unchanged against a real SQL Server / LocalDB instance.
/// </summary>
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

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxAttempts, int batchSize, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        // UPDLOCK, READPAST lets multiple competing processors claim disjoint batches without blocking.
        IEnumerable<OutboxMessage> messages = await connection.QueryAsync<OutboxMessage>(
            new CommandDefinition(
                """
                SELECT Id, Type, Payload, OccurredOnUtc, ProcessedOnUtc, Attempts, Error
                FROM OutboxMessages WITH (UPDLOCK, READPAST)
                WHERE ProcessedOnUtc IS NULL AND Attempts < @MaxAttempts
                ORDER BY OccurredOnUtc
                OFFSET 0 ROWS FETCH NEXT @BatchSize ROWS ONLY;
                """,
                new { MaxAttempts = maxAttempts, BatchSize = batchSize },
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        return [.. messages];
    }

    public async Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE OutboxMessages
                SET ProcessedOnUtc = @ProcessedOnUtc, Attempts = @Attempts, Error = @Error
                WHERE Id = @Id;
                """,
                new { message.Id, message.ProcessedOnUtc, message.Attempts, message.Error },
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
                INSERT INTO OutboxMessages (Id, Type, Payload, OccurredOnUtc, ProcessedOnUtc, Attempts, Error)
                VALUES (@Id, @Type, @Payload, @OccurredOnUtc, @ProcessedOnUtc, @Attempts, @Error);
                """,
                new
                {
                    message.Id,
                    message.Type,
                    message.Payload,
                    message.OccurredOnUtc,
                    message.ProcessedOnUtc,
                    message.Attempts,
                    message.Error
                },
                transaction,
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);
    }

    #endregion
}
