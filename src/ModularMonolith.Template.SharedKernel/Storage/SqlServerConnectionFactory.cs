using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace ModularMonolith.Template.SharedKernel.Storage;

public sealed class SqlServerConnectionFactory : IDbConnectionFactory
{
    #region Field Declarations

    private static readonly SqlRetryLogicBaseProvider _retryProvider = SqlConfigurableRetryFactory.CreateExponentialRetryProvider
    (
        new SqlRetryLogicOption
        {
            NumberOfTries = 5,
            DeltaTime = TimeSpan.FromMilliseconds(200),
            MaxTimeInterval = TimeSpan.FromSeconds(10)
        })
    ;

    private readonly Func<DbConnection> _connectionFactory;

    #endregion

    #region Property Declarations

    public string ConnectionString { get; }

    #endregion

    #region Constructor Declarations

    public SqlServerConnectionFactory(string connectionString) : this(connectionString, connectionFactory: null)
    {
    }

    internal SqlServerConnectionFactory(string connectionString, Func<DbConnection>? connectionFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        SqlConnectionStringBuilder builder = new(connectionString);
        ConnectionString = builder.ConnectionString;
        _connectionFactory = connectionFactory ?? CreateSqlConnection;
    }

    #endregion

    #region Public Method Declarations

    public async Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        DbConnection connection = _connectionFactory();

        try
        {
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    #endregion

    #region Private Method Declarations

    private SqlConnection CreateSqlConnection()
    {
        SqlConnection connection = new(ConnectionString)
        {
            RetryLogicProvider = _retryProvider
        };
        return connection;
    }

    #endregion
}
