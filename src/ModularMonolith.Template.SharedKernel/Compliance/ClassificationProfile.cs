using System.Linq.Expressions;

namespace ModularMonolith.Template.SharedKernel.Compliance;

public abstract class ClassificationProfile<T>
{
    #region Constructor Declarations

    protected ClassificationProfile()
    {
    }

    #endregion

    #region Private Method Declarations

    protected void Classify<TProperty>(Expression<Func<T, TProperty>> selector, DataClassification classification)
    {
        _ = selector;
        _ = classification;
    }

    #endregion
}