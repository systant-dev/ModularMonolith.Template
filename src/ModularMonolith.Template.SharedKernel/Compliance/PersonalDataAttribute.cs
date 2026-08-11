using Microsoft.Extensions.Compliance.Classification;

namespace ModularMonolith.Template.SharedKernel.Compliance;

public sealed class PersonalDataAttribute : DataClassificationAttribute
{
    #region Constructor Declarations

    public PersonalDataAttribute() : base(DataTaxonomy.PersonalData)
    {
    }

    #endregion
}