namespace AccountsModule.Contracts.Requests;

public sealed record UpdateAccountRequest
{
    #region Property Declarations

    public required string AccountHolderName { get; init; }

    #endregion
}
