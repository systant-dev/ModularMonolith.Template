namespace ModularMonolith.Template.SharedKernel.Outbox;

public interface IModuleDatabaseInitialiser
{
    #region Method Declarations

    Task InitialiseAsync(CancellationToken cancellationToken = default);

    #endregion
}
