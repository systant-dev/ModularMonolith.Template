using System.Linq.Expressions;

namespace ModularMonolith.Template.SharedKernel.Compliance;

/// <summary>
/// Base type for fluent classification profiles. Derive from this type and call
/// <see cref="Classify{TProperty}"/> in the constructor to declare which members are sensitive.
/// The ComplianceGen source generator reads those calls as syntax and emits an
/// <see cref="IModelRedactor{T}"/> implementation for the profile (the profile must be
/// declared <see langword="partial"/>).
/// </summary>
/// <typeparam name="T">The model type described by this profile.</typeparam>
public abstract class ClassificationProfile<T>
{
    /// <summary>
    /// Declares the classification of a model member. The body is intentionally empty: it is never
    /// executed. The source generator reads the call statically to emit a strongly-typed redactor.
    /// </summary>
    /// <param name="selector">A simple or nested member-access selector, for example <c>x =&gt; x.Email</c>.</param>
    /// <param name="classification">The classification to apply to the selected member.</param>
    protected void Classify<TProperty>(Expression<Func<T, TProperty>> selector, DataClassification classification)
    {
        // Intentionally empty: the call exists purely for the source generator to read statically.
        _ = selector;
        _ = classification;
    }
}
