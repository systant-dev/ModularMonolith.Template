using Microsoft.Extensions.Logging;

namespace ModularMonolith.Template.SharedKernel.Rules;

public sealed class RuleProcessor : IRuleProcessor
{
    #region Field Declarations

    private readonly ILogger<RuleProcessor> _logger;

    #endregion

    #region Constructor Declarations

    public RuleProcessor(ILogger<RuleProcessor> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    #endregion

    #region Public Method Declarations

    public async Task RunRulesAsync(IEnumerable<IRule> rules, CancellationToken cancellationToken)
    {
        if (!rules.Any())
        {
            throw new ArgumentException("Rules collection cannot be empty.", nameof(rules));
        }

        List<IRule> orderedRules = [.. rules.OrderBy(rule => rule.Order)];
        orderedRules.ForEach(async rule =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (rule.CanExecute())
            {
                RuleProcessorLog.ExecutingRule(_logger, rule.GetType().Name);
                await rule.ExecuteAsync(cancellationToken);
            }
        });
    }

    #endregion
}
