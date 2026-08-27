namespace ModularMonolith.Template.SharedKernel.Rules;

public interface IRule
{
    #region Property Declarations

    int Order { get; }

    #endregion

    #region Method Declarations

    bool CanExecute();

    Task ExecuteAsync(CancellationToken cancellationToken);

    #endregion
}
