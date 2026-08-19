using Microsoft.Extensions.Logging;

namespace ModularMonolith.Template.SharedKernel.Idempotency;

internal static partial class IdempotencyFilterLog
{
    #region Public Method Declarations

    [LoggerMessage(Level = LogLevel.Warning, Message = "Idempotency rejected: missing {HeaderName} header")]
    public static partial void LogMissingIdempotencyKey(this ILogger logger, string headerName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Idempotency rejected: duplicate key {IdempotencyKey}")]
    public static partial void LogDuplicateIdempotencyKey(this ILogger logger, string idempotencyKey);

    #endregion
}
