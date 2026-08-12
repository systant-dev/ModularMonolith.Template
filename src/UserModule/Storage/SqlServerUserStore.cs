using Dapper;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;
using ModularMonolith.Template.SharedKernel.Storage;
using System.Data.Common;
using UserModule.Models;

namespace UserModule.Storage;

internal sealed class SqlServerUserStore(IDbConnectionFactory connectionFactory) : SqlServerOutboxStore(connectionFactory), IUserStore
{
    #region IUserStore Method Declarations

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        CommandDefinition command = new("SELECT Id, FullName, Email, PhoneNumber FROM Users ORDER BY Id;", cancellationToken: cancellationToken);
        IEnumerable<User> users = await connection.QueryAsync<User>(command);
        return [.. users];
    }

    public async Task<IReadOnlyList<User>> GetPageAsync(int afterId, int limit, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = new { AfterId = afterId, Limit = limit };
        // Keyset (seek) pagination: cheaper and more stable than OFFSET as the table grows because it
        // seeks straight to the last-seen Id instead of counting and skipping rows.
        CommandDefinition command = new(
            "SELECT TOP (@Limit) Id, FullName, Email, PhoneNumber FROM Users WHERE Id > @AfterId ORDER BY Id;",
            parameters,
            cancellationToken: cancellationToken);
        IEnumerable<User> users = await connection.QueryAsync<User>(command);
        return [.. users];
    }

    public async Task<User?> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = new { Id = id };
        CommandDefinition command = new("SELECT Id, FullName, Email, PhoneNumber FROM Users WHERE Id = @Id;", parameters, cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<User>(command);
    }

    public async Task<User> CreateAsync(User user, Func<User, IMessage> integrationEventFactory, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using DbTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        var parameters = new { user.FullName, user.Email, user.PhoneNumber };
        CommandDefinition command = new(
            """
            INSERT INTO Users (FullName, Email, PhoneNumber)
            OUTPUT INSERTED.Id
            VALUES (@FullName, @Email, @PhoneNumber);
            """,
            parameters,
            transaction,
            cancellationToken: cancellationToken);

        int id = await connection.ExecuteScalarAsync<int>(command);
        User created = user with { Id = id };
        await InsertOutboxMessageAsync(connection, transaction, OutboxMessage.Create(integrationEventFactory(created)), cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return created;
    }

    public async Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = new { user.Id, user.FullName, user.Email, user.PhoneNumber };
        CommandDefinition command = new(
            """
            UPDATE Users
            SET FullName = @FullName, Email = @Email, PhoneNumber = @PhoneNumber
            WHERE Id = @Id;
            """,
            parameters,
            cancellationToken: cancellationToken);

        int affected = await connection.ExecuteAsync(command);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = new { Id = id };
        CommandDefinition command = new("DELETE FROM Users WHERE Id = @Id;", parameters, cancellationToken: cancellationToken);
        int affected = await connection.ExecuteAsync(command);
        return affected > 0;
    }

    #endregion
}
