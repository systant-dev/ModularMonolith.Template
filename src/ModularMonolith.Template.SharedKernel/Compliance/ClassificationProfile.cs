using System;
using System.Linq.Expressions;

namespace ModularMonolith.Template.SharedKernel.Compliance;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ClassificationProfileAttribute : Attribute
{
}

public abstract class ClassificationProfile<T>
{
    #region Private Method Declarations

    protected void Classify<TProperty>(Expression<Func<T, TProperty>> selector, DataClassification classification)
    {
        _ = selector;
        _ = classification;
    }

    #endregion
}
