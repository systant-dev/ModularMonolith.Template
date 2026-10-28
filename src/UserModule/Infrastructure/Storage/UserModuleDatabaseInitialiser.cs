using Dapper;
using ModularMonolith.Template.SharedKernel.Outbox;
using ModularMonolith.Template.SharedKernel.Storage;
using System.Data.Common;

namespace UserModule.Infrastructure.Storage;

internal sealed class UserModuleDatabaseInitialiser(IDbConnectionFactory connectionFactory) : IModuleDatabaseInitialiser
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

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Users_Email' AND object_id = OBJECT_ID(N'dbo.Users', N'U'))
        BEGIN
            CREATE UNIQUE INDEX UX_Users_Email ON dbo.Users (Email);
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
