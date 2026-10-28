using Dapper;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;
using System.Data.Common;

namespace ModularMonolith.Template.SharedKernel.Storage;

public sealed class SqlServerOutboxInterceptor : IOutboxInterceptor
{
    public async Task InterceptAsync(DbConnection connection,
                                      DbTransaction? transaction,
                                      IMessage message,
                                      CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(message);

        OutboxMessage outboxMessage = OutboxMessage.Create(message);
        var parameters = new
        {
            outboxMessage.Id,
            outboxMessage.Type,
            outboxMessage.Payload,
            outboxMessage.OccurredOnUtc,
            outboxMessage.ProcessedOnUtc,
            outboxMessage.Attempts,
            outboxMessage.Error,
            outboxMessage.NextAttemptOnUtc
        };

        CommandDefinition command = new(
            """
            INSERT INTO OutboxMessages (Id, Type, Payload, OccurredOnUtc, ProcessedOnUtc, Attempts, Error, NextAttemptOnUtc)
            VALUES (@Id, @Type, @Payload, @OccurredOnUtc, @ProcessedOnUtc, @Attempts, @Error, @NextAttemptOnUtc);
            """,
            parameters,
            transaction,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }
}
