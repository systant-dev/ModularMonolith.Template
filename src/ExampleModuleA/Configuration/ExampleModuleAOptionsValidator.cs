using FluentValidation;

namespace ExampleModuleA.Configuration;

internal sealed class ExampleModuleAOptionsValidator : AbstractValidator<ExampleModuleAOptions>
{
    #region Constructor Declarations

    public ExampleModuleAOptionsValidator()
    {
    }

    #endregion
}