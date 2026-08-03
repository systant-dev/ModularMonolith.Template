namespace ModularMonolith.Template.SharedKernel.Compliance;

/// <summary>
/// Well-known data sensitivity classifications used to drive redaction.
/// </summary>
public enum DataClassification
{
    /// <summary>The value carries no sensitivity and is never redacted.</summary>
    None = 0,

    /// <summary>Public information that may be shared freely.</summary>
    Public = 1,

    /// <summary>Internal information that should not leave the organisation.</summary>
    Internal = 2,

    /// <summary>Personal information (for example a name or email address).</summary>
    Personal = 3,

    /// <summary>Highly sensitive information (for example a national identifier).</summary>
    Sensitive = 4,
}
