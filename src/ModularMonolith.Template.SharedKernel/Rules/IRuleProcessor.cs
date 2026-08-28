using ModularMonolith.Template.SharedKernel.Results;

namespace ModularMonolith.Template.SharedKernel.Rules;

public interface IRuleProcessor<in TContext>
{
    #region Method Declarations

    Task<Result> RunRulesAsync(TContext context, CancellationToken cancellationToken);

    #endregion
}
