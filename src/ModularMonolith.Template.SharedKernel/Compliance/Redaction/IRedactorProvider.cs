namespace ModularMonolith.Template.SharedKernel.Compliance.Redaction;

public interface IRedactorProvider
{
    #region Method Declarations

    IStringRedactor GetRedactor(DataClassification classification);

    #endregion
}
