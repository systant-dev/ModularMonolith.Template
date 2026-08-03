using FluentAssertions;
using ModularMonolith.Template.SharedKernel.Compliance;
using ModularMonolith.Template.SharedKernel.Compliance.Redaction;

namespace ModularMonolith.Template.SharedKernel.Tests.Compliance;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Compliance)]
public sealed class RedactorProviderTests
{
    #region Test Method Declarations

    [Fact]
    public void GetRedactor_ShouldReturnConfiguredRedactor()
    {
        ErasingRedactor erasing = new();
        ComplianceOptions options = new();
        options.SetRedactor(DataClassification.Personal, erasing);
        RedactorProvider provider = new(options);

        provider.GetRedactor(DataClassification.Personal).Should().BeSameAs(erasing);
    }

    [Fact]
    public void GetRedactor_ShouldReturnFallback_WhenClassificationNotConfigured()
    {
        ComplianceOptions options = new();
        RedactorProvider provider = new(options);

        provider.GetRedactor(DataClassification.Sensitive).Should().BeSameAs(PassthroughRedactor.Instance);
    }

    [Fact]
    public void GetRedactor_ShouldReturnCustomFallback()
    {
        ErasingRedactor fallback = new();
        ComplianceOptions options = new() { FallbackRedactor = fallback };
        RedactorProvider provider = new(options);

        provider.GetRedactor(DataClassification.Internal).Should().BeSameAs(fallback);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOptionsIsNull()
    {
        Action act = () => _ = new RedactorProvider(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}
