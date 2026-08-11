using Dapper;
using ModularMonolith.Template.SharedKernel.Outbox;
using ModularMonolith.Template.SharedKernel.Storage;
using System.Data.Common;

namespace AccountsModule.Storage;

internal sealed class AccountsModuleDatabaseInitialiser(IDbConnectionFactory connectionFactory) : IModuleDatabaseInitialiser
{
    #region Constant Declarations

    private const string CreateSchemaSql =
        """
        IF OBJECT_ID(N'dbo.Accounts', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.Accounts (
                Id                INT             NOT NULL IDENTITY(1, 1) PRIMARY KEY,
                UserId            INT             NOT NULL,
                AccountNumber     NVARCHAR(64)    NOT NULL,
                AccountHolderName NVARCHAR(256)   NOT NULL,
                Balance           DECIMAL(19, 4)  NOT NULL CONSTRAINT DF_Accounts_Balance DEFAULT 0
            );

            CREATE INDEX IX_Accounts_UserId ON dbo.Accounts (UserId);
        END;
        """
        + "\n\n" + OutboxSchema.CreateTableSql;

    #endregion

    #region Public Method Declarations

    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        CommandDefinition commandDefinition = new(CreateSchemaSql, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(commandDefinition);
    }

    #endregion
}
