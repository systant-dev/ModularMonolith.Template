namespace ModularMonolith.Template.SharedKernel.Outbox;

/// <summary>
/// Initialises a module's database (schema/tables) at startup. Implementations may run EF Core
/// migrations, execute raw DDL for Dapper-backed stores, or perform any other setup required
/// before the module can serve requests.
/// </summary>
public interface IModuleDatabaseInitialiser
{
    #region Method Declarations

    Task InitialiseAsync(CancellationToken cancellationToken = default);

    #endregion
}
