namespace ModularMonolith.Template.SharedKernel.Outbox;

public static class OutboxSchema
{
    #region Constant Declarations

    public const string CreateTableSql =
        """
        IF OBJECT_ID(N'dbo.OutboxMessages', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.OutboxMessages (
                Sequence         BIGINT             NOT NULL IDENTITY(1, 1),
                Id               UNIQUEIDENTIFIER   NOT NULL CONSTRAINT PK_OutboxMessages PRIMARY KEY NONCLUSTERED,
                Type             NVARCHAR(1024)     NOT NULL,
                Payload          NVARCHAR(MAX)      NOT NULL,
                OccurredOnUtc    DATETIMEOFFSET     NOT NULL,
                ProcessedOnUtc   DATETIMEOFFSET     NULL,
                Attempts         INT                NOT NULL CONSTRAINT DF_OutboxMessages_Attempts DEFAULT 0,
                Error            NVARCHAR(MAX)      NULL,
                NextAttemptOnUtc DATETIMEOFFSET     NULL,
                INDEX CX_OutboxMessages_Sequence UNIQUE CLUSTERED (Sequence)
            );
        END;

        IF COL_LENGTH(N'dbo.OutboxMessages', N'NextAttemptOnUtc') IS NULL
        BEGIN
            ALTER TABLE dbo.OutboxMessages ADD NextAttemptOnUtc DATETIMEOFFSET NULL;
        END;

        DECLARE @ClusteredOutboxPrimaryKey SYSNAME =
        (
            SELECT primaryKey.name
            FROM sys.key_constraints AS primaryKey
            INNER JOIN sys.indexes AS primaryKeyIndex
                ON primaryKeyIndex.object_id = primaryKey.parent_object_id
               AND primaryKeyIndex.index_id = primaryKey.unique_index_id
            WHERE primaryKey.parent_object_id = OBJECT_ID(N'dbo.OutboxMessages', N'U')
              AND primaryKey.type = N'PK'
              AND primaryKeyIndex.type = 1
        );

        IF @ClusteredOutboxPrimaryKey IS NOT NULL
        BEGIN
            DECLARE @DropClusteredOutboxPrimaryKey NVARCHAR(MAX) = N'ALTER TABLE dbo.OutboxMessages DROP CONSTRAINT ' + QUOTENAME(@ClusteredOutboxPrimaryKey) + N';';
            EXEC sys.sp_executesql @DropClusteredOutboxPrimaryKey;
        END;

        IF COL_LENGTH(N'dbo.OutboxMessages', N'Sequence') IS NULL
        BEGIN
            ALTER TABLE dbo.OutboxMessages ADD Sequence BIGINT NOT NULL IDENTITY(1, 1);
        END;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes
                       WHERE name = N'CX_OutboxMessages_Sequence'
                         AND object_id = OBJECT_ID(N'dbo.OutboxMessages', N'U'))
        BEGIN
            EXEC sys.sp_executesql N'CREATE UNIQUE CLUSTERED INDEX CX_OutboxMessages_Sequence ON dbo.OutboxMessages (Sequence);';
        END;

        IF NOT EXISTS (SELECT 1 FROM sys.key_constraints
                       WHERE parent_object_id = OBJECT_ID(N'dbo.OutboxMessages', N'U')
                         AND type = N'PK')
        BEGIN
            ALTER TABLE dbo.OutboxMessages ADD CONSTRAINT PK_OutboxMessages PRIMARY KEY NONCLUSTERED (Id);
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
