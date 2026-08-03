namespace UserModule.Contracts.Requests;

public sealed record UpdateUserRequest
{
    #region Property Declarations

    public required string FullName { get; init; }

    public required string Email { get; init; }

    public string? PhoneNumber { get; init; }

    #endregion
}