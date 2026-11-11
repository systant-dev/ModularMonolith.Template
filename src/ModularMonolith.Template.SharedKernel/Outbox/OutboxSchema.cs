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
        END;

        IF COL_LENGTH(N'dbo.OutboxMessages', N'NextAttemptOnUtc') IS NULL
        BEGIN
            ALTER TABLE dbo.OutboxMessages ADD NextAttemptOnUtc DATETIMEOFFSET NULL;
        END;

        -- Drop the legacy non-filtered index so it can be replaced by the filtered covering index below.
        IF EXISTS (SELECT 1 FROM sys.indexes
                   WHERE name = N'IX_OutboxMessages_Pending'
                     AND object_id = OBJECT_ID(N'dbo.OutboxMessages')
                     AND has_filter = 0)
        BEGIN
            DROP INDEX IX_OutboxMessages_Pending ON dbo.OutboxMessages;
        END;

        -- Filtered covering index: only indexes unprocessed rows and covers the claim query's OUTPUT
        -- columns so GetPendingAsync can seek/scan a small index instead of the full table.
        IF NOT EXISTS (SELECT 1 FROM sys.indexes
                       WHERE name = N'IX_OutboxMessages_Pending'
                         AND object_id = OBJECT_ID(N'dbo.OutboxMessages'))
        BEGIN
            CREATE INDEX IX_OutboxMessages_Pending
                ON dbo.OutboxMessages (NextAttemptOnUtc, OccurredOnUtc)
                INCLUDE (Type, Payload, Attempts, Error, ProcessedOnUtc)
                WHERE ProcessedOnUtc IS NULL;
        END;
        """;

    #endregion
}
