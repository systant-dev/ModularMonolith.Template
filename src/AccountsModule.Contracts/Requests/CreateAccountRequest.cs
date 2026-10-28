namespace AccountsModule.Contracts.Requests;

public sealed record CreateAccountRequest
{
    #region Property Declarations

    public required int UserId { get; init; }

    public required string AccountHolderName { get; init; }

    public decimal InitialDeposit { get; init; }

    #endregion
}
