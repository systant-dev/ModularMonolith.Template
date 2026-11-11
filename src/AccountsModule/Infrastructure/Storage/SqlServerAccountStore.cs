using AccountsModule.Features.Accounts;
using AccountsModule.Infrastructure.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;
using ModularMonolith.Template.SharedKernel.Results;
using ModularMonolith.Template.SharedKernel.Storage;
using System.Data.Common;

namespace AccountsModule.Infrastructure.Storage;

internal sealed class SqlServerAccountStore([FromKeyedServices(Module.ServiceKey)] IDbConnectionFactory connectionFactory,
                                            IOutboxInterceptor outboxInterceptor) : SqlServerOutboxStore(connectionFactory, outboxInterceptor), IAccountStore
{
    #region Constant Declarations

    private const string AccountNumberIndexName = "UX_Accounts_AccountNumber";

    #endregion

    #region Public Method Declarations

    public async Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        CommandDefinition command = new("SELECT Id, UserId, AccountNumber, AccountHolderName, Balance FROM Accounts ORDER BY Id;", cancellationToken: cancellationToken);
        IEnumerable<Account> accounts = await connection.QueryAsync<Account>(command);
        return [.. accounts];
    }

    public async Task<IReadOnlyList<Account>> GetPageAsync(int afterId, int limit, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = new { AfterId = afterId, Limit = limit };
        CommandDefinition command = new(
            "SELECT TOP (@Limit) Id, UserId, AccountNumber, AccountHolderName, Balance FROM Accounts WHERE Id > @AfterId ORDER BY Id;",
            parameters,
            cancellationToken: cancellationToken);
        IEnumerable<Account> accounts = await connection.QueryAsync<Account>(command);
        return [.. accounts];
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        CommandDefinition command = new("SELECT COUNT_BIG(*) FROM Accounts;", cancellationToken: cancellationToken);
        return (int)await connection.ExecuteScalarAsync<long>(command);
    }

    public async Task<Account?> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = new { Id = id };
        CommandDefinition command = new("SELECT Id, UserId, AccountNumber, AccountHolderName, Balance FROM Accounts WHERE Id = @Id;", parameters, cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Account>(command);
    }

    public async Task<bool> ExistsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = new { UserId = userId };
        CommandDefinition command = new(
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM Accounts WHERE UserId = @UserId) THEN 1 ELSE 0 END;",
            parameters,
            cancellationToken: cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<Result<Account>> CreateAsync(Account account, Func<Account, IMessage> integrationEventFactory, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using DbTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        var parameters = new { account.UserId, account.AccountNumber, account.AccountHolderName, account.Balance };
        CommandDefinition command = new(
            """
            INSERT INTO Accounts (UserId, AccountNumber, AccountHolderName, Balance)
            OUTPUT INSERTED.Id
            VALUES (@UserId, @AccountNumber, @AccountHolderName, @Balance);
            """,
            parameters,
            transaction,
            cancellationToken: cancellationToken);

        try
        {
            int id = await connection.ExecuteScalarAsync<int>(command);
            Account created = account with { Id = id };
            await OutboxInterceptor.InterceptAsync(connection, transaction, integrationEventFactory(created), cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return created;
        }
        catch (SqlException exception) when (IsUniqueAccountConflict(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return exception.Message.Contains(AccountNumberIndexName, StringComparison.Ordinal)
                ? Error.Conflict("account.number-conflict", $"Account number '{account.AccountNumber}' is already in use.")
                : Error.Conflict(AccountErrorCodes.AlreadyExists,$"An account already exists for user {account.UserId}.");
        }
    }

    public async Task<Result<Account>> UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = new { account.Id, account.AccountHolderName };
        CommandDefinition command = new(
            """
            UPDATE Accounts
            SET AccountHolderName = @AccountHolderName
            OUTPUT INSERTED.Id, INSERTED.UserId, INSERTED.AccountNumber, INSERTED.AccountHolderName, INSERTED.Balance
            WHERE Id = @Id;
            """,
            parameters,
            cancellationToken: cancellationToken);

        Account? updated = await connection.QuerySingleOrDefaultAsync<Account>(command);
        return updated is not null
            ? updated
            : Error.NotFound("account.not-found", $"Account {account.Id} was not found.");
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = new { Id = id };
        CommandDefinition command = new("DELETE FROM Accounts WHERE Id = @Id;", parameters, cancellationToken: cancellationToken);
        int affected = await connection.ExecuteAsync(command);
        return affected > 0;
    }

    #endregion

    #region Private Method Declarations

    private static bool IsUniqueAccountConflict(SqlException exception)
    {
        return exception.Number is 2601 or 2627
            && (exception.Message.Contains(AccountNumberIndexName, StringComparison.Ordinal)
                || exception.Message.Contains("UX_Accounts_UserId", StringComparison.Ordinal));
    }

    #endregion
}
