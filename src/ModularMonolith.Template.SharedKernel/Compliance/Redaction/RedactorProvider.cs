namespace ModularMonolith.Template.SharedKernel.Compliance.Redaction;

public sealed class RedactorProvider(ComplianceOptions options) : IRedactorProvider
{
    #region Field Declarations

    private readonly ComplianceOptions _options = options ?? throw new ArgumentNullException(nameof(options));

    #endregion

    #region Public Method Declarations

    public IStringRedactor GetRedactor(DataClassification classification) => _options.Resolve(classification);

    #endregion
}