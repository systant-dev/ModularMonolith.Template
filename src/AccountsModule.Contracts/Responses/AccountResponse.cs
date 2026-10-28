namespace AccountsModule.Contracts.Responses;

public sealed record AccountResponse
{
    #region Property Declarations

    public required int Id { get; init; }

    public required int UserId { get; init; }

    public required string AccountNumber { get; init; }

    public required string AccountHolderName { get; init; }

    public required decimal Balance { get; init; }

    #endregion
}
