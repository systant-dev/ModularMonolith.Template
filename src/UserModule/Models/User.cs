namespace UserModule.Models;

internal sealed partial record User
{
    #region Property Declarations

    public required int Id { get; init; }

    public required string FullName { get; set; }

    public required string Email { get; set; }

    public string? PhoneNumber { get; set; }

    #endregion
}
