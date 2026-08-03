namespace ModularMonolith.Template.SharedKernel.Compliance;

/// <summary>
/// A redactor that replaces the entire value with a fixed mask, erasing the original content.
/// </summary>
public sealed class ErasingRedactor : IStringRedactor
{
    #region Field Declarations

    /// <summary>A shared instance using the default mask.</summary>
    public static readonly ErasingRedactor Instance = new();

    private readonly string _mask;

    #endregion

    #region Constructor Declarations

    /// <summary>Creates a redactor that erases values with the supplied mask.</summary>
    /// <param name="mask">The replacement text. Defaults to <c>***</c>.</param>
    public ErasingRedactor(string mask = "***")
    {
        _mask = mask ?? string.Empty;
    }

    #endregion

    #region Public Method Declarations

    public string Redact(string? value) => string.IsNullOrEmpty(value) ? string.Empty : _mask;

    #endregion
}
