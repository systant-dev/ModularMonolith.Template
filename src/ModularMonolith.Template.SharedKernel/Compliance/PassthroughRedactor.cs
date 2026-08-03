namespace ModularMonolith.Template.SharedKernel.Compliance;

public sealed class PassthroughRedactor : IStringRedactor
{
    #region Field Declarations

    public static readonly PassthroughRedactor Instance = new();

    #endregion

    #region Public Method Declarations

    public string Redact(string? value) => value ?? string.Empty;

    #endregion
}
