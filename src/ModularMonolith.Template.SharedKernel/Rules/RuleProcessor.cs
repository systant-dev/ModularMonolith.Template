using Microsoft.Extensions.Logging;
using ModularMonolith.Template.SharedKernel.Results;

namespace ModularMonolith.Template.SharedKernel.Rules;

public sealed class RuleProcessor<TContext> : IRuleProcessor<TContext>
{
    #region Field Declarations

    private readonly IRule<TContext>[] _rules;
    private readonly ILogger<RuleProcessor<TContext>> _logger;

    #endregion

    #region Constructor Declarations

    public RuleProcessor(IEnumerable<IRule<TContext>> rules, ILogger<RuleProcessor<TContext>> logger)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(logger);
        _rules = [.. rules.OrderBy(rule => rule.Order)];
        _logger = logger;
    }

    #endregion

    #region Public Method Declarations

    public async Task<Result> RunRulesAsync(TContext context, CancellationToken cancellationToken)
    {
        RuleProcessorLog.RulesReceived(_logger, string.Join(", ", _rules.Select(rule => rule.GetType().Name)));

        foreach (IRule<TContext> rule in _rules)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (rule.CanExecute(context))
            {
                RuleProcessorLog.ExecutingRule(_logger, rule.GetType().Name);
                Result result = await rule.ExecuteAsync(context, cancellationToken);
                if (result.IsFailure)
                {
                    RuleProcessorLog.RuleFailed(_logger, rule.GetType().Name, result.Error.Code, result.Error.Message);
                    return result;
                }
            }
        }

        return Result.Success();
    }

    #endregion
}
