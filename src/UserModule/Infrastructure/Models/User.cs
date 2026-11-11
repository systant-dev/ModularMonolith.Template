
using ModularMonolith.Template.SharedKernel.Compliance;

namespace UserModule.Infrastructure.Models;

internal sealed record User
{
    #region Property Declarations

    public int Id { get; init; }

    [PersonalData]
    public required string FullName { get; set; }

    [SensitiveData]
    public required string Email { get; set; }

    [PersonalData]
    public string? PhoneNumber { get; set; }

    #endregion
}
