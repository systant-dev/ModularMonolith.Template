namespace ModularMonolith.Template.SharedKernel.Inbox;

public static class InboxSchema
{
    #region Constant Declarations

    public const string CreateTableSql =
        """
        IF OBJECT_ID(N'dbo.InboxMessages', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.InboxMessages (
                Consumer      NVARCHAR(256)     NOT NULL,
                MessageId     NVARCHAR(256)     NOT NULL,
                ProcessedOnUtc DATETIMEOFFSET   NOT NULL CONSTRAINT DF_InboxMessages_ProcessedOnUtc DEFAULT SYSUTCDATETIME(),
                CONSTRAINT PK_InboxMessages PRIMARY KEY (Consumer, MessageId)
            );
        END;
        """;

    #endregion
}
