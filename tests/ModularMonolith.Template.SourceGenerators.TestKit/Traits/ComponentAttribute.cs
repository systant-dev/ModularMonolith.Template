using Xunit.v3;

namespace ModularMonolith.Template.SourceGenerators.TestKit.Traits;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class ComponentAttribute(TestComponent component) : Attribute, ITraitAttribute
{
    #region Field Declarations

    public const string TraitName = "Component";

    #endregion

    #region Property Declarations

    public TestComponent Component { get; } = component;

    #endregion

    #region Public Method Declarations

    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
    {
        KeyValuePair<string, string> trait = new(TraitName, Component.ToString());
        return [trait];
    }

    #endregion
}