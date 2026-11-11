using Microsoft.Extensions.Logging;

namespace ModularMonolith.Template.SharedKernel.Validation;

internal static partial class ValidationFilterLog
{
    #region Public Method Declarations

    [LoggerMessage(Level = LogLevel.Warning, Message = "Validation failed for {RequestType}")]
    public static partial void LogValidationFailed(this ILogger logger, string requestType);

    #endregion
}
