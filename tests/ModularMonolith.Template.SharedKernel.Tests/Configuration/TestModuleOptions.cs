using FluentValidation;
using ModularMonolith.Template.SharedKernel.Configuration;

namespace ModularMonolith.Template.SharedKernel.Tests.Configuration;

public sealed record TestModuleOptions : ModuleOptions
{
    public string Name { get; init; } = string.Empty;
}

public sealed class TestModuleOptionsValidator : AbstractValidator<TestModuleOptions>
{
    public TestModuleOptionsValidator()
    {
        RuleFor(options => options.Name).NotEmpty();
    }
}