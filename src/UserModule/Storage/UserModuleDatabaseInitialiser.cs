using Dapper;
using Microsoft.Data.SqlClient;
using ModularMonolith.Template.SharedKernel.Outbox;

namespace UserModule.Storage;

/// <summary>
/// Creates the SQL Server tables required by the UserModule (users and the transactional outbox)
/// at startup. Runs idempotent T-SQL DDL because the module uses Dapper rather than an EF Core
/// migration pipeline. In production this responsibility typically moves to a migration tool.
/// </summary>
internal sealed class UserModuleDatabaseInitialiser(SqlServerConnectionFactory connectionFactory) : IModuleDatabaseInitialiser
{
    #region Constant Declarations

    private const string CreateSchemaSql =
        """
        IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.Users (
                Id          INT             NOT NULL IDENTITY(1, 1) PRIMARY KEY,
                FullName    NVARCHAR(256)   NOT NULL,
                Email       NVARCHAR(256)   NOT NULL,
                PhoneNumber NVARCHAR(64)    NULL
            );
        END;

        IF OBJECT_ID(N'dbo.OutboxMessages', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.OutboxMessages (
                Id             UNIQUEIDENTIFIER   NOT NULL PRIMARY KEY,
                Type           NVARCHAR(1024)     NOT NULL,
                Payload        NVARCHAR(MAX)      NOT NULL,
                OccurredOnUtc  DATETIMEOFFSET     NOT NULL,
                ProcessedOnUtc DATETIMEOFFSET     NULL,
                Attempts       INT                NOT NULL CONSTRAINT DF_OutboxMessages_Attempts DEFAULT 0,
                Error          NVARCHAR(MAX)      NULL
            );

            CREATE INDEX IX_OutboxMessages_ProcessedOnUtc
                ON dbo.OutboxMessages (ProcessedOnUtc);
        END;
        """;

    #endregion

    #region Public Method Declarations

    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(CreateSchemaSql, cancellationToken: cancellationToken))
            .ConfigureAwait(false);
    }

    #endregion
}
