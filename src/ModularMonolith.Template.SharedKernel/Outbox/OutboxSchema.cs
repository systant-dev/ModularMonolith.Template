namespace ModularMonolith.Template.SharedKernel.Outbox;

public static class OutboxSchema
{
    #region Constant Declarations

    public const string CreateTableSql =
        """
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

            CREATE INDEX IX_OutboxMessages_Pending
                ON dbo.OutboxMessages (ProcessedOnUtc, Attempts, NextAttemptOnUtc);
        END;

        IF COL_LENGTH(N'dbo.OutboxMessages', N'NextAttemptOnUtc') IS NULL
        BEGIN
            ALTER TABLE dbo.OutboxMessages ADD NextAttemptOnUtc DATETIMEOFFSET NULL;
        END;
        """;

    #endregion
}
