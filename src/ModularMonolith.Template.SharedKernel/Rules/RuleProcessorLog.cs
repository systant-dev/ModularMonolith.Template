using Microsoft.Extensions.Logging;

namespace ModularMonolith.Template.SharedKernel.Rules;

internal static partial class RuleProcessorLog
{
    #region Log Message Declarations

    [LoggerMessage(LogLevel.Information, "Executing rule {Rule}")]
    public static partial void ExecutingRule(ILogger logger, string rule);

    [LoggerMessage(LogLevel.Information, "Rules received: {Rules}")]
    public static partial void RulesReceived(ILogger logger, string rules);

    [LoggerMessage(LogLevel.Warning, "Rule {Rule} failed with error {ErrorCode}: {ErrorMessage}")]
    public static partial void RuleFailed(ILogger logger, string rule, string errorCode, string errorMessage);

    #endregion
}
