namespace ModularMonolith.Template.SharedKernel.Rules;

public interface IRuleProcessor
{
    #region Method Declarations

    Task RunRulesAsync(IEnumerable<IRule> rules, CancellationToken cancellationToken)

    #endregion
}
