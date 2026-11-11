using Xunit.v3;

namespace ModularMonolith.Template.SourceGenerators.TestKit.Traits;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class CategoryAttribute(TestCategory category) : Attribute, ITraitAttribute
{
    #region Field Declarations

    public const string TraitName = "Category";

    #endregion

    #region Property Declarations

    public TestCategory Category { get; } = category;

    #endregion

    #region Public Method Declarations

    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
    {
        KeyValuePair<string, string> trait = new(TraitName, Category.ToString());
        return [trait];
    }

    #endregion
}
