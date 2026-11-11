namespace ModularMonolith.Template.SharedKernel.Inbox;

public static class InboxSchema
{
    #region Constant Declarations

    public const string CreateTableSql =
        """
        IF OBJECT_ID(N'dbo.InboxMessages', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.InboxMessages (
                Consumer      NVARCHAR(128)     NOT NULL,
                MessageId     NVARCHAR(128)     NOT NULL,
                ProcessedOnUtc DATETIMEOFFSET   NOT NULL CONSTRAINT DF_InboxMessages_ProcessedOnUtc DEFAULT SYSUTCDATETIME(),
                CONSTRAINT PK_InboxMessages PRIMARY KEY (Consumer, MessageId)
            );
        END;

        IF (COL_LENGTH(N'dbo.InboxMessages', N'Consumer') > 256 OR COL_LENGTH(N'dbo.InboxMessages', N'MessageId') > 256)
           AND NOT EXISTS (SELECT 1 FROM dbo.InboxMessages WHERE DATALENGTH(Consumer) > 256 OR DATALENGTH(MessageId) > 256)
        BEGIN
            IF OBJECT_ID(N'dbo.PK_InboxMessages', N'PK') IS NOT NULL
            BEGIN
                ALTER TABLE dbo.InboxMessages DROP CONSTRAINT PK_InboxMessages;
            END;

            ALTER TABLE dbo.InboxMessages ALTER COLUMN Consumer NVARCHAR(128) NOT NULL;
            ALTER TABLE dbo.InboxMessages ALTER COLUMN MessageId NVARCHAR(128) NOT NULL;
        END;

        IF NOT EXISTS (SELECT 1 FROM sys.key_constraints
                       WHERE parent_object_id = OBJECT_ID(N'dbo.InboxMessages', N'U')
                         AND type = N'PK')
        BEGIN
            ALTER TABLE dbo.InboxMessages ADD CONSTRAINT PK_InboxMessages PRIMARY KEY (Consumer, MessageId);
        END;
        """;

    #endregion
}
