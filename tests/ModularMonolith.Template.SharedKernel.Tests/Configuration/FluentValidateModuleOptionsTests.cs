using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;
using ModularMonolith.Template.SharedKernel.Configuration;
using Moq;

namespace ModularMonolith.Template.SharedKernel.Tests.Configuration;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Configuration)]
public sealed class FluentValidateModuleOptionsTests
{
    #region Test Method Declarations

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenValidatorIsNull()
    {
        Action act = () => _ = new FluentValidateModuleOptions<TestModuleOptions>(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("validator");
    }

    [Fact]
    public void Validate_ShouldThrowArgumentNullException_WhenOptionsIsNull()
    {
        Mock<IValidator<TestModuleOptions>> validator = new();
        FluentValidateModuleOptions<TestModuleOptions> sut = new(validator.Object);

        Action act = () => sut.Validate("name", null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenValidationPasses()
    {
        Mock<IValidator<TestModuleOptions>> validator = new();
        TestModuleOptions options = new() { Enabled = true, Name = "Module" };
        ValidationResult validationResult = new();
        validator.Setup(v => v.Validate(options)).Returns(validationResult);

        FluentValidateModuleOptions<TestModuleOptions> sut = new(validator.Object);

        ValidateOptionsResult result = sut.Validate(null, options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenValidationFails()
    {
        Mock<IValidator<TestModuleOptions>> validator = new();
        TestModuleOptions options = new() { Enabled = true, Name = string.Empty };
        ValidationResult validationResult = new(
        [
            new ValidationFailure(nameof(TestModuleOptions.Name), "Name is required")
        ]);
        validator.Setup(v => v.Validate(options)).Returns(validationResult);

        FluentValidateModuleOptions<TestModuleOptions> sut = new(validator.Object);

        ValidateOptionsResult result = sut.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
              .Which.Should().Be($"{nameof(TestModuleOptions)}.{nameof(TestModuleOptions.Name)}: Name is required");
    }

    [Fact]
    public void Validate_ShouldIncludeAllFailures_WhenMultipleValidationErrors()
    {
        Mock<IValidator<TestModuleOptions>> validator = new();
        TestModuleOptions options = new() { Enabled = true, Name = string.Empty };
        ValidationResult validationResult = new(
        [
            new ValidationFailure("Name", "Name is required"),
            new ValidationFailure("Enabled", "Enabled must be true")
        ]);
        validator.Setup(v => v.Validate(options)).Returns(validationResult);

        FluentValidateModuleOptions<TestModuleOptions> sut = new(validator.Object);

        ValidateOptionsResult result = sut.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().HaveCount(2);
    }

    #endregion
}
