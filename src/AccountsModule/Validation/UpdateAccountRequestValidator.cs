using FluentValidation;
using AccountsModule.Contracts.Requests;

namespace AccountsModule.Validation;

internal sealed class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
    #region Constructor Declarations

    public UpdateAccountRequestValidator()
    {
        RuleFor(request => request.AccountHolderName).NotEmpty().MaximumLength(200);
    }

    #endregion
}
