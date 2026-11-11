using AccountsModule.Contracts.Requests;
using FluentValidation;

namespace AccountsModule.Features.Accounts.Validation;

internal sealed class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    #region Constructor Declarations

    public CreateAccountRequestValidator()
    {
        RuleFor(request => request.UserId).GreaterThan(0);
        RuleFor(request => request.AccountHolderName).NotEmpty().MaximumLength(200);
        RuleFor(request => request.InitialDeposit).GreaterThanOrEqualTo(0);
    }

    #endregion
}
