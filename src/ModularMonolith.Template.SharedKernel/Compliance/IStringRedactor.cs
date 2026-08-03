namespace ModularMonolith.Template.SharedKernel.Compliance;

/// <summary>
/// Redacts a single string value for a particular <see cref="DataClassification"/>.
/// </summary>
public interface IStringRedactor
{
    /// <summary>Redacts the supplied value. Implementations must tolerate <see langword="null"/>.</summary>
    string Redact(string? value);
}
