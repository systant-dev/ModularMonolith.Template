using FluentValidation;
using ModularMonolith.Template.SharedKernel.Configuration;

namespace ModularMonolith.Template.SharedKernel.Tests.Configuration;

public sealed record TestModuleOptions : ModuleOptions
{
    #region Property Declarations

    public string Name { get; init; } = string.Empty;

    #endregion
}

public sealed class TestModuleOptionsValidator : AbstractValidator<TestModuleOptions>
{
    #region Constructor Declarations

    public TestModuleOptionsValidator()
    {
        RuleFor(options => options.Name).NotEmpty();
    }

    #endregion
}