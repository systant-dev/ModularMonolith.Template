using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModularMonolith.Template.SharedKernel.Configuration;

namespace ModularMonolith.Template.SharedKernel.Tests.Configuration;

public sealed class OptionsServiceCollectionExtensionsTests
{
    #region Test Method Declarations

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddValidatedOptions_ShouldThrow_WhenSectionNameIsNullOrWhiteSpace(string? sectionName)
    {
        ServiceCollection services = new();

        Action act = () => services.AddValidatedOptions<TestModuleOptions, TestModuleOptionsValidator>(sectionName!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddValidatedOptions_ShouldReturnSameServiceCollection()
    {
        ServiceCollection services = new();

        IServiceCollection result = services.AddValidatedOptions<TestModuleOptions, TestModuleOptionsValidator>("Section");

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddValidatedOptions_ShouldRegisterValidator()
    {
        ServiceCollection services = new();

        services.AddValidatedOptions<TestModuleOptions, TestModuleOptionsValidator>("Section");

        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IValidator<TestModuleOptions>) &&
            descriptor.ImplementationType == typeof(TestModuleOptionsValidator) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddValidatedOptions_ShouldRegisterValidateOptions()
    {
        ServiceCollection services = new();

        services.AddValidatedOptions<TestModuleOptions, TestModuleOptionsValidator>("Section");

        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IValidateOptions<TestModuleOptions>) &&
            descriptor.ImplementationType == typeof(FluentValidateModuleOptions<TestModuleOptions>) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddValidatedOptions_ShouldResolveValidateOptions()
    {
        ServiceCollection services = new();
        services.AddValidatedOptions<TestModuleOptions, TestModuleOptionsValidator>("Section");

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetService<IValidateOptions<TestModuleOptions>>()
                .Should().BeOfType<FluentValidateModuleOptions<TestModuleOptions>>();
    }

    #endregion
}