using Microsoft.Data.SqlClient;

namespace UserModule.Storage;

/// <summary>
/// Creates connections to the module's SQL Server database. The example targets SQL Server
/// LocalDB (Windows-only) so it runs without a container, but any SQL Server connection string
/// works in production. Connections are lightweight and pooled by the provider, so a new open
/// connection is created per operation.
/// </summary>
internal sealed class SqlServerConnectionFactory(string connectionString)
{
    #region Public Method Declarations

    public string ConnectionString => connectionString;

    public async Task<SqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }

    #endregion
}
