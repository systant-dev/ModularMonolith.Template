using Microsoft.Data.SqlClient;
using Dapper;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;
using ModularMonolith.Template.SharedKernel.Storage;
using ModularMonolith.Template.SharedKernel.Results;
using System.Data.Common;
using UserModule.Infrastructure.Models;

namespace UserModule.Infrastructure.Storage;

internal sealed class SqlServerUserStore(IDbConnectionFactory connectionFactory,
                                         IOutboxInterceptor outboxInterceptor) : SqlServerOutboxStore(connectionFactory, outboxInterceptor), IUserStore
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
        CommandDefinition command = new(
            "SELECT TOP (@Limit) Id, FullName, Email, PhoneNumber FROM Users WHERE Id > @AfterId ORDER BY Id;",
            parameters,
            cancellationToken: cancellationToken);
        IEnumerable<User> users = await connection.QueryAsync<User>(command);
        return [.. users];
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        CommandDefinition command = new("SELECT COUNT_BIG(*) FROM Users;", cancellationToken: cancellationToken);
        return (int)await connection.ExecuteScalarAsync<long>(command);
    }

    public async Task<User?> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = new { Id = id };
        CommandDefinition command = new("SELECT Id, FullName, Email, PhoneNumber FROM Users WHERE Id = @Id;", parameters, cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<User>(command);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = new { Email = email };
        CommandDefinition command = new(
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM Users WHERE Email = @Email) THEN 1 ELSE 0 END;",
            parameters,
            cancellationToken: cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<bool> ExistsByEmailAsync(string email, int excludingUserId, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = new { Email = email, ExcludingUserId = excludingUserId };
        CommandDefinition command = new(
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM Users WHERE Email = @Email AND Id <> @ExcludingUserId) THEN 1 ELSE 0 END;",
            parameters,
            cancellationToken: cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<Result<User>> CreateAsync(User user, Func<User, IMessage> integrationEventFactory, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using DbTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            User created = await InsertUserAsync(connection, transaction, user, cancellationToken);
            await OutboxInterceptor.InterceptAsync(connection, transaction, integrationEventFactory(created), cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return created;
        }
        catch (SqlException exception) when (IsEmailConflict(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return Error.Conflict("user.email-conflict", $"A user with email '{user.Email}' already exists.");
        }
    }

    public async Task<Result<User>> UpdateAsync(User user, Func<User, IMessage> integrationEventFactory, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using DbTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            int affected = await UpdateUserAsync(connection, transaction, user, cancellationToken);
            if (affected == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Error.NotFound("user.not-found", $"User {user.Id} was not found.");
            }

            await OutboxInterceptor.InterceptAsync(connection, transaction, integrationEventFactory(user), cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return user;
        }
        catch (SqlException exception) when (IsEmailConflict(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return Error.Conflict("user.email-conflict", $"A user with email '{user.Email}' already exists.");
        }
    }

    public async Task<bool> DeleteAsync(int id, Func<int, IMessage> integrationEventFactory, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using DbTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        int affected = await DeleteUserAsync(connection, transaction, id, cancellationToken);
        if (affected == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        await OutboxInterceptor.InterceptAsync(connection, transaction, integrationEventFactory(id), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    #endregion

    #region Private Method Declarations

    private static async Task<User> InsertUserAsync(DbConnection connection,
                                                    DbTransaction transaction,
                                                    User user,
                                                    CancellationToken cancellationToken)
    {
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
        return user with { Id = id };
    }

    private static Task<int> UpdateUserAsync(DbConnection connection,
                                             DbTransaction transaction,
                                             User user,
                                             CancellationToken cancellationToken)
    {
        var parameters = new { user.Id, user.FullName, user.Email, user.PhoneNumber };
        CommandDefinition command = new(
            """
            UPDATE Users
            SET FullName = @FullName, Email = @Email, PhoneNumber = @PhoneNumber
            WHERE Id = @Id;
            """,
            parameters,
            transaction,
            cancellationToken: cancellationToken);

        return connection.ExecuteAsync(command);
    }

    private static Task<int> DeleteUserAsync(DbConnection connection,
                                             DbTransaction transaction,
                                             int id,
                                             CancellationToken cancellationToken)
    {
        var parameters = new { Id = id };
        CommandDefinition command = new(
            "DELETE FROM Users WHERE Id = @Id;",
            parameters,
            transaction,
            cancellationToken: cancellationToken);

        return connection.ExecuteAsync(command);
    }

    private static bool IsEmailConflict(SqlException exception)
    {
        return exception.Number is 2601 or 2627
            && exception.Message.Contains("UX_Users_Email", StringComparison.Ordinal);
    }

    #endregion
}
