using Dapper;
using Microsoft.Data.SqlClient;
using ModularMonolith.Template.SharedKernel.Outbox;

namespace AccountsModule.Storage;

internal sealed class AccountsModuleDatabaseInitialiser(SqlServerConnectionFactory connectionFactory) : IModuleDatabaseInitialiser
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

        IF OBJECT_ID(N'dbo.OutboxMessages', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.OutboxMessages (
                Id               UNIQUEIDENTIFIER   NOT NULL PRIMARY KEY,
                Type             NVARCHAR(1024)     NOT NULL,
                Payload          NVARCHAR(MAX)      NOT NULL,
                OccurredOnUtc    DATETIMEOFFSET     NOT NULL,
                ProcessedOnUtc   DATETIMEOFFSET     NULL,
                Attempts         INT                NOT NULL CONSTRAINT DF_OutboxMessages_Attempts DEFAULT 0,
                Error            NVARCHAR(MAX)      NULL,
                NextAttemptOnUtc DATETIMEOFFSET     NULL
            );

            CREATE INDEX IX_OutboxMessages_Pending ON dbo.OutboxMessages (ProcessedOnUtc, Attempts, NextAttemptOnUtc);
        END;

        IF COL_LENGTH(N'dbo.OutboxMessages', N'NextAttemptOnUtc') IS NULL
        BEGIN
            ALTER TABLE dbo.OutboxMessages ADD NextAttemptOnUtc DATETIMEOFFSET NULL;
        END;
        """;

    #endregion

    #region Public Method Declarations

    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(CreateSchemaSql, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    #endregion
}
