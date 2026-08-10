
using ModularMonolith.Template.SharedKernel.Compliance;

namespace AccountsModule.Models;

internal sealed record Account
{
    #region Property Declarations

    public int Id { get; init; }

    public required int UserId { get; init; }

    public string AccountNumber { get; set; } = string.Empty;

    [PersonalData]
    public required string AccountHolderName { get; set; }

    public decimal Balance { get; set; }

    #endregion
}
