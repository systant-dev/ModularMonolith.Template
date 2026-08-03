namespace ModularMonolith.Template.SharedKernel.Compliance;

public sealed class ComplianceOptions
{
    #region Field Declarations

    private readonly Dictionary<DataClassification, IStringRedactor> _redactors = new();

    #endregion

    #region Property Declarations

    public IStringRedactor FallbackRedactor { get; set; } = PassthroughRedactor.Instance;

    #endregion

    #region Public Method Declarations

    public ComplianceOptions SetRedactor(DataClassification classification, IStringRedactor redactor)
    {
        ArgumentNullException.ThrowIfNull(redactor);
        _redactors[classification] = redactor;
        return this;
    }

    public IStringRedactor Resolve(DataClassification classification) =>
        _redactors.TryGetValue(classification, out IStringRedactor? redactor) ? redactor : FallbackRedactor;

    #endregion
}
