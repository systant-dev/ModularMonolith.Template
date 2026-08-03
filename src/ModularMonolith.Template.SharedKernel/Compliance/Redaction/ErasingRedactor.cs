namespace ModularMonolith.Template.SharedKernel.Compliance.Redaction;

public sealed class ErasingRedactor(string mask = "***") : IStringRedactor
{
    #region Field Declarations

    public static readonly ErasingRedactor Instance = new();

    private readonly string _mask = mask ?? string.Empty;

    #endregion

    #region Public Method Declarations

    public string Redact(string? value) => string.IsNullOrEmpty(value) ? string.Empty : _mask;

    #endregion
}