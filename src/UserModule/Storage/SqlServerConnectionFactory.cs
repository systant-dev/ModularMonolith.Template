using Microsoft.Data.SqlClient;

namespace UserModule.Storage;

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
