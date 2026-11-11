using Microsoft.Extensions.Logging;

namespace ModularMonolith.Template.SharedKernel.Auditing;

internal static partial class AuditLogFilterLog
{
    #region Public Method Declarations

    [LoggerMessage(Level = LogLevel.Information, Message = "Audit: {User} performed {Method} {Path} responding {StatusCode}")]
    public static partial void LogAuditAction(this ILogger logger, string user, string method, string path, int statusCode);

    #endregion
}
