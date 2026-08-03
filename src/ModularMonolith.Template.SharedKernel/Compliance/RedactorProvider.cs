namespace ModularMonolith.Template.SharedKernel.Compliance;

public sealed class RedactorProvider : IRedactorProvider
{
    #region Field Declarations

    private readonly ComplianceOptions _options;

    #endregion

    #region Constructor Declarations

    public RedactorProvider(ComplianceOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    #endregion

    #region Public Method Declarations

    public IStringRedactor GetRedactor(DataClassification classification) => _options.Resolve(classification);

    #endregion
}
