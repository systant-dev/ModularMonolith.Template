using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace ModularMonolith.Template.SharedKernel.Storage;

public sealed class SqlServerConnectionFactory : IDbConnectionFactory
{
    #region Field Declarations

    // Transient-fault retry provider shared across connections. Handles the well-known transient
    // SQL/Azure SQL error numbers (throttling, failovers, transport faults) with capped exponential
    // backoff so callers don't have to implement their own retry loops.
    private static readonly SqlRetryLogicBaseProvider RetryProvider = SqlConfigurableRetryFactory.CreateExponentialRetryProvider(
        new SqlRetryLogicOption
        {
            NumberOfTries = 5,
            DeltaTime = TimeSpan.FromMilliseconds(200),
            MaxTimeInterval = TimeSpan.FromSeconds(10)
        });

    #endregion

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
        SqlConnection connection = new(ConnectionString)
        {
            RetryLogicProvider = RetryProvider
        };
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    #endregion
}