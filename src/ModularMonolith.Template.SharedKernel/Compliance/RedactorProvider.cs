namespace ModularMonolith.Template.SharedKernel.Compliance;

/// <summary>
/// The default <see cref="IRedactorProvider"/>. Resolves redactors from a configured
/// <see cref="ComplianceOptions"/> instance.
/// </summary>
public sealed class RedactorProvider : IRedactorProvider
{
    #region Field Declarations

    private readonly ComplianceOptions _options;

    #endregion

    #region Constructor Declarations

    /// <summary>Creates a provider backed by the supplied options.</summary>
    public RedactorProvider(ComplianceOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    #endregion

    #region Public Method Declarations

    public IStringRedactor GetRedactor(DataClassification classification) => _options.Resolve(classification);

    #endregion
}
