
using ModularMonolith.Template.SharedKernel.Compliance;

namespace UserModule.Models;

internal sealed partial record User
{
    #region Property Declarations

    public required int Id { get; init; }

    [PersonalData]
    public required string FullName { get; set; }

    [SensitiveData]
    public required string Email { get; set; }

    [PersonalData]
    public string? PhoneNumber { get; set; }

    #endregion
}