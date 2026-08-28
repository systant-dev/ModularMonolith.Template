using ModularMonolith.Template.SharedKernel.Results;

namespace ModularMonolith.Template.SharedKernel.Rules;

public interface IRule<in TContext>
{
    #region Property Declarations

    int Order { get; }

    #endregion

    #region Method Declarations

    bool CanExecute(TContext context);

    Task<Result> ExecuteAsync(TContext context, CancellationToken cancellationToken);

    #endregion
}
