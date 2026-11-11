using Dapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ModularMonolith.Template.SharedKernel.Storage;
using System.Data.Common;

namespace ModularMonolith.Template.SharedKernel.HealthChecks;

internal sealed class DbConnectionHealthCheck(IDbConnectionFactory connectionFactory) : IHealthCheck
{
    #region Public Method Declarations

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await using DbConnection connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
            CommandDefinition command = new("SELECT 1;", cancellationToken: cancellationToken);
            await connection.ExecuteScalarAsync<int>(command);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "The database could not be reached.", exception);
        }
    }

    #endregion
}
