namespace ModularMonolith.Template.SharedKernel.Compliance;

/// <summary>
/// Resolves the <see cref="IStringRedactor"/> associated with a <see cref="DataClassification"/>.
/// </summary>
public interface IRedactorProvider
{
    /// <summary>Gets the redactor configured for the supplied classification.</summary>
    IStringRedactor GetRedactor(DataClassification classification);
}
