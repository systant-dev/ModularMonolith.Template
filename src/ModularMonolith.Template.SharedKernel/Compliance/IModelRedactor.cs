namespace ModularMonolith.Template.SharedKernel.Compliance;

/// <summary>
/// A generated redactor for a specific model type. Implementations are produced by the
/// ComplianceGen source generator from a <see cref="ClassificationProfile{T}"/>.
/// </summary>
/// <typeparam name="T">The model type whose classified members are redacted.</typeparam>
public interface IModelRedactor<T>
{
    /// <summary>Redacts every classified member of <paramref name="model"/> in place and returns it.</summary>
    T Redact(T model, IRedactorProvider redactorProvider);
}
