using FluentValidation;

namespace UserModule.Infrastructure.Configuration;

internal sealed class UserModuleOptionsValidator : AbstractValidator<UserModuleOptions>
{
    #region Constructor Declarations

    public UserModuleOptionsValidator()
    {
        RuleFor(options => options.MaxUsers).GreaterThan(0);
        RuleFor(options => options.ConnectionString).NotEmpty();
    }

    #endregion
}
