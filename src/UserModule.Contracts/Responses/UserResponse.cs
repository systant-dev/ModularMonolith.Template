namespace UserModule.Contracts.Responses;

public sealed record UserResponse
{
    #region Property Declarations

    public required int Id { get; init; }

    public required string FullName { get; init; }

    public required string Email { get; init; }

    public string? PhoneNumber { get; init; }

    #endregion
}
