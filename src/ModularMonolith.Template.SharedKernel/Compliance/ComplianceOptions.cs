namespace ModularMonolith.Template.SharedKernel.Compliance;

/// <summary>
/// Configures which <see cref="IStringRedactor"/> is applied to each <see cref="DataClassification"/>.
/// </summary>
public sealed class ComplianceOptions
{
    #region Field Declarations

    private readonly Dictionary<DataClassification, IStringRedactor> _redactors = new();

    #endregion

    #region Property Declarations

    /// <summary>
    /// The redactor used when no explicit redactor is configured for a classification.
    /// Defaults to <see cref="PassthroughRedactor.Instance"/>.
    /// </summary>
    public IStringRedactor FallbackRedactor { get; set; } = PassthroughRedactor.Instance;

    #endregion

    #region Public Method Declarations

    /// <summary>Maps a classification to the redactor that should process its values.</summary>
    public ComplianceOptions SetRedactor(DataClassification classification, IStringRedactor redactor)
    {
        ArgumentNullException.ThrowIfNull(redactor);
        _redactors[classification] = redactor;
        return this;
    }

    /// <summary>Resolves the redactor for a classification, falling back to <see cref="FallbackRedactor"/>.</summary>
    public IStringRedactor Resolve(DataClassification classification) =>
        _redactors.TryGetValue(classification, out IStringRedactor? redactor) ? redactor : FallbackRedactor;

    #endregion
}
