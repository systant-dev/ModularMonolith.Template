using AccountsModule.Contracts.Requests;
using FluentValidation;

namespace AccountsModule.Features.Accounts.Validation;

internal sealed class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
    #region Constructor Declarations

    public UpdateAccountRequestValidator()
    {
        RuleFor(request => request.AccountHolderName).NotEmpty().MaximumLength(200);
    }

    #endregion
}
