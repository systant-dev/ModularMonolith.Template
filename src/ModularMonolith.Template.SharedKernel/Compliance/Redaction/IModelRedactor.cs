namespace ModularMonolith.Template.SharedKernel.Compliance.Redaction;

public interface IModelRedactor<T>
{
    #region Method Declarations

    T Redact(T model, IRedactorProvider redactorProvider);

    #endregion
}
