using FluentValidation;

namespace AccountsModule.Infrastructure.Configuration;

internal sealed class AccountsModuleOptionsValidator : AbstractValidator<AccountsModuleOptions>
{
    #region Constructor Declarations

    public AccountsModuleOptionsValidator()
    {
        RuleFor(options => options.MaxAccounts).GreaterThan(0);
        RuleFor(options => options.ConnectionString).NotEmpty();
    }

    #endregion
}
