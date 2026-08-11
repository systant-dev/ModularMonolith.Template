using Microsoft.Extensions.Compliance.Classification;

namespace ModularMonolith.Template.SharedKernel.Compliance;

public sealed class SensitiveDataAttribute : DataClassificationAttribute
{
    #region Constructor Declarations

    public SensitiveDataAttribute() : base(DataTaxonomy.SensitiveData)
    {
    }

    #endregion
}
