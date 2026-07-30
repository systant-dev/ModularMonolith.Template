using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;

namespace ModularMonolith.Template.SharedKernel.Configuration;

public sealed class FluentValidateModuleOptions<TModuleOptions> : IValidateOptions<TModuleOptions> where TModuleOptions : class, IModuleOptions
{
    #region Field Declarations

    private readonly IValidator<TModuleOptions> _validator;

    #endregion

    #region Constructor Declarations

    public FluentValidateModuleOptions(IValidator<TModuleOptions> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);
        _validator = validator;
    }

    #endregion

    #region Public Method Declarations

    public ValidateOptionsResult Validate(string? name, TModuleOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        ValidationResult result = _validator.Validate(options);
        if (result.IsValid)
        {
            return ValidateOptionsResult.Success;
        }

        IEnumerable<string> failures = result.Errors.Select(failure => $"{typeof(TModuleOptions).Name}.{failure.PropertyName}: {failure.ErrorMessage}");

        return ValidateOptionsResult.Fail(failures);
    }

    #endregion
}