using System.Data.Common;

namespace ModularMonolith.Template.SharedKernel.Storage;

public interface IDbConnectionFactory
{
    #region Method Declarations

    Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);

    #endregion
}
