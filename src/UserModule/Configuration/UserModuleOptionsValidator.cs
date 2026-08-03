using FluentValidation;

namespace UserModule.Configuration;

internal sealed class UserModuleOptionsValidator : AbstractValidator<UserModuleOptions>
{
    #region Constructor Declarations

    public UserModuleOptionsValidator()
    {
        RuleFor(options => options.MaxUsers)
            .GreaterThan(0);
    }

    #endregion
}
