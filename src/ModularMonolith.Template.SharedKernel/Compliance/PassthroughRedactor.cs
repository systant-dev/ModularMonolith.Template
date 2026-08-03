namespace ModularMonolith.Template.SharedKernel.Compliance;

/// <summary>
/// A redactor that returns the value unchanged. Used as the default for non-sensitive
/// classifications such as <see cref="DataClassification.None"/> and <see cref="DataClassification.Public"/>.
/// </summary>
public sealed class PassthroughRedactor : IStringRedactor
{
    #region Field Declarations

    /// <summary>A shared, thread-safe instance.</summary>
    public static readonly PassthroughRedactor Instance = new();

    #endregion

    #region Public Method Declarations

    public string Redact(string? value) => value ?? string.Empty;

    #endregion
}
