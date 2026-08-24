namespace AccountsModule.Storage;

/// <summary>
/// Thrown when inserting an account violates the unique index on <c>AccountNumber</c>. Signals to the
/// service layer that a freshly generated account number collided and a new one should be generated.
/// </summary>
internal sealed class DuplicateAccountNumberException(string accountNumber, Exception innerException)
    : Exception($"An account with number '{accountNumber}' already exists.", innerException)
{
    #region Property Declarations

    public string AccountNumber { get; } = accountNumber;

    #endregion
}
