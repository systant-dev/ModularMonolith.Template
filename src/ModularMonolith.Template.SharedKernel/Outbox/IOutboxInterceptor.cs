using ModularMonolith.Template.SharedKernel.Messaging;
using System.Data.Common;

namespace ModularMonolith.Template.SharedKernel.Outbox;

public interface IOutboxInterceptor
{
    Task InterceptAsync(DbConnection connection,
                         DbTransaction? transaction,
                         IMessage message,
                         CancellationToken cancellationToken = default);
}
