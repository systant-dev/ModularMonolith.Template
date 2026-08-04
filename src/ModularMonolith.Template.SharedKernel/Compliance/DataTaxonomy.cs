using Microsoft.Extensions.Compliance.Classification;

namespace ModularMonolith.Template.SharedKernel.Compliance;

public static class DataTaxonomy
{
    #region Property Declarations

    public static string TaxonomyName => typeof(DataTaxonomy).FullName!;

    public static DataClassification PersonalData => new(TaxonomyName, nameof(PersonalData));

    public static DataClassification SensitiveData => new(TaxonomyName, nameof(SensitiveData));

    #endregion
}
