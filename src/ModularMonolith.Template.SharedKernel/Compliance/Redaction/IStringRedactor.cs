namespace ModularMonolith.Template.SharedKernel.Compliance.Redaction;

public interface IStringRedactor
{
    #region Method Declarations

    string Redact(string? value);

    #endregion
}
