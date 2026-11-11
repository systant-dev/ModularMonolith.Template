using Dapper;
using ModularMonolith.Template.SharedKernel.Inbox;
using System.Data.Common;

namespace ModularMonolith.Template.SharedKernel.Storage;

public sealed class SqlServerInboxStore(IDbConnectionFactory connectionFactory) : IInboxStore
{
    #region Public Method Declarations

    public async Task<bool> IsProcessedAsync(string consumer, string messageId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consumer);
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);

        await using DbConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new { Consumer = consumer, MessageId = messageId };
        CommandDefinition command = new(
            """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.InboxMessages
                WHERE Consumer = @Consumer AND MessageId = @MessageId) THEN 1 ELSE 0 END;
            """,
            parameters,
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<bool> TryMarkProcessedAsync(string consumer, string messageId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consumer);
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);

        await using DbConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new { Consumer = consumer, MessageId = messageId };
        CommandDefinition command = new(
            """
            INSERT INTO dbo.InboxMessages (Consumer, MessageId)
            SELECT @Consumer, @MessageId
            WHERE NOT EXISTS (
                SELECT 1 FROM dbo.InboxMessages WITH (UPDLOCK, HOLDLOCK)
                WHERE Consumer = @Consumer AND MessageId = @MessageId);
            """,
            parameters,
            cancellationToken: cancellationToken);

        int inserted = await connection.ExecuteAsync(command);
        return inserted > 0;
    }

    public async Task<int> PurgeProcessedAsync(DateTimeOffset processedBefore, int batchSize, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new { ProcessedBefore = processedBefore, BatchSize = batchSize };
        CommandDefinition command = new(
            """
            DELETE TOP (@BatchSize) FROM dbo.InboxMessages
            WHERE ProcessedOnUtc < @ProcessedBefore;
            """,
            parameters,
            cancellationToken: cancellationToken);

        return await connection.ExecuteAsync(command);
    }

    #endregion
}
