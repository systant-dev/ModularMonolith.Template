using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Compliance;
using System.Reflection;

namespace ModularMonolith.Template.SharedKernel.Tests.Compliance;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Compliance)]
public sealed class ComplianceServiceCollectionExtensionsTests
{
    #region Test Method Declarations

    [Fact]
    public void AddCompliance_ShouldReturnSameServiceCollection()
    {
        ServiceCollection services = new();

        IServiceCollection result = services.AddCompliance();

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddCompliance_ShouldRegisterProviderAndOptions()
    {
        ServiceCollection services = new();

        services.AddCompliance(options => options.SetRedactor(DataClassification.Personal, new ErasingRedactor()));

        using ServiceProvider provider = services.BuildServiceProvider();
        IRedactorProvider redactorProvider = provider.GetRequiredService<IRedactorProvider>();

        redactorProvider.GetRedactor(DataClassification.Personal).Should().BeOfType<ErasingRedactor>();
    }

    [Fact]
    public void AddCompliance_ShouldDiscoverModelRedactorsInAssembly()
    {
        ServiceCollection services = new();

        services.AddCompliance(assemblies: Assembly.GetExecutingAssembly());

        using ServiceProvider provider = services.BuildServiceProvider();
        IModelRedactor<SampleModel> redactor = provider.GetRequiredService<IModelRedactor<SampleModel>>();

        redactor.Should().BeOfType<SampleModelRedactor>();
    }

    [Fact]
    public void AddCompliance_DiscoveredRedactor_ShouldRedactUsingProvider()
    {
        ServiceCollection services = new();
        services.AddCompliance(
            options => options.SetRedactor(DataClassification.Personal, new ErasingRedactor()),
            Assembly.GetExecutingAssembly());

        using ServiceProvider provider = services.BuildServiceProvider();
        IModelRedactor<SampleModel> redactor = provider.GetRequiredService<IModelRedactor<SampleModel>>();
        IRedactorProvider redactorProvider = provider.GetRequiredService<IRedactorProvider>();

        SampleModel result = redactor.Redact(new SampleModel { Name = "Ada Lovelace" }, redactorProvider);

        result.Name.Should().Be("***");
    }

    #endregion

    #region Nested Types

    public sealed class SampleModel
    {
        public string Name { get; set; } = string.Empty;
    }

    public sealed class SampleModelRedactor : IModelRedactor<SampleModel>
    {
        public SampleModel Redact(SampleModel model, IRedactorProvider redactorProvider)
        {
            model.Name = redactorProvider.GetRedactor(DataClassification.Personal).Redact(model.Name);
            return model;
        }
    }

    #endregion
}
