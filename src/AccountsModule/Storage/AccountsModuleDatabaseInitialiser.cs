using Dapper;
using ModularMonolith.Template.SharedKernel.Inbox;
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
        END;

        -- Drop the legacy non-unique index so it can be replaced by the unique index that backs the
        -- one-account-per-user invariant enforced by AccountService.ExistsForUserAsync.
        IF EXISTS (SELECT 1 FROM sys.indexes
                   WHERE name = N'IX_Accounts_UserId'
                     AND object_id = OBJECT_ID(N'dbo.Accounts', N'U')
                     AND is_unique = 0)
        BEGIN
            DROP INDEX IX_Accounts_UserId ON dbo.Accounts;
        END;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Accounts_UserId' AND object_id = OBJECT_ID(N'dbo.Accounts', N'U'))
        BEGIN
            CREATE UNIQUE INDEX UX_Accounts_UserId ON dbo.Accounts (UserId);
        END;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Accounts_AccountNumber' AND object_id = OBJECT_ID(N'dbo.Accounts', N'U'))
        BEGIN
            CREATE UNIQUE INDEX UX_Accounts_AccountNumber ON dbo.Accounts (AccountNumber);
        END;
        """
        + "\n\n" + OutboxSchema.CreateTableSql
        + "\n\n" + InboxSchema.CreateTableSql;

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
