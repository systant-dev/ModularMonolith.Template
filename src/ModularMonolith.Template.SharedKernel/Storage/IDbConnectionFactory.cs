using System.Data.Common;

namespace ModularMonolith.Template.SharedKernel.Storage;

public interface IDbConnectionFactory
{
    Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
