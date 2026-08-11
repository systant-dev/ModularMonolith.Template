using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace ModularMonolith.Template.SharedKernel.Storage;

public sealed class SqlServerConnectionFactory : IDbConnectionFactory
{
    #region Property Declarations

    public string ConnectionString { get; }

    #endregion

    #region Constructor Declarations

    public SqlServerConnectionFactory(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        SqlConnectionStringBuilder builder = new(connectionString);
        ConnectionString = builder.ConnectionString;
    }

    #endregion

    #region Public Method Declarations

    public async Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        SqlConnection connection = new(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    #endregion
}