namespace ModularMonolith.Template.SharedKernel.Compliance;

public interface IRedactorProvider
{
    #region Method Declarations

    IStringRedactor GetRedactor(DataClassification classification);

    #endregion
}
