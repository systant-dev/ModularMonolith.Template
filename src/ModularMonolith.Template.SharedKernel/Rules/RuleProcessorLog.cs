using Microsoft.Extensions.Logging;

namespace ModularMonolith.Template.SharedKernel.Rules;

internal static partial class RuleProcessorLog
{
    #region Log Message Declarations

    [LoggerMessage(LogLevel.Information, "Executing rule {Rule}")]
    public static partial void ExecutingRule(ILogger logger, string rule);

    #endregion
}
