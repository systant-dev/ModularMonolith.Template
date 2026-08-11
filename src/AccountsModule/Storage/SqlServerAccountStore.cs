using AccountsModule.Models;
using Dapper;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;
using ModularMonolith.Template.SharedKernel.Storage;
using System.Data.Common;

namespace AccountsModule.Storage;

internal sealed class SqlServerAccountStore(IDbConnectionFactory connectionFactory) : SqlServerOutboxStore(connectionFactory), IAccountStore
{
    #region IAccountStore Method Declarations

    public async Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        CommandDefinition command = new("SELECT Id, UserId, AccountNumber, AccountHolderName, Balance FROM Accounts ORDER BY Id;", cancellationToken: cancellationToken);
        IEnumerable<Account> accounts = await connection.QueryAsync<Account>(command);
        return [.. accounts];
    }

    public async Task<Account?> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = new { Id = id };
        CommandDefinition command = new("SELECT Id, UserId, AccountNumber, AccountHolderName, Balance FROM Accounts WHERE Id = @Id;", parameters, cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Account>(command);
    }

    public async Task<Account> CreateAsync(Account account, Func<Account, IMessage> integrationEventFactory, CancellationToken cancellationToken = default)
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

        int id = await connection.ExecuteScalarAsync<int>(command);
        Account created = account with { Id = id };
        await InsertOutboxMessageAsync(connection, transaction, OutboxMessage.Create(integrationEventFactory(created)), cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return created;
    }

    public async Task<bool> UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await ConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var parameters = new { account.Id, account.AccountHolderName, account.Balance };
        CommandDefinition command = new(
            """
            UPDATE Accounts
            SET AccountHolderName = @AccountHolderName, Balance = @Balance
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
        CommandDefinition command = new("DELETE FROM Accounts WHERE Id = @Id;", parameters, cancellationToken: cancellationToken);
        int affected = await connection.ExecuteAsync(command);
        return affected > 0;
    }

    #endregion
}
