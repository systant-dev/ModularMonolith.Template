using FluentValidation;
using UserModule.Contracts.Requests;

namespace UserModule.Validation;

internal sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    #region Constructor Declarations

    public CreateUserRequestValidator()
    {
        RuleFor(request => request.FullName).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(request => request.PhoneNumber).MaximumLength(30).When(request => request.PhoneNumber is not null);
    }

    #endregion
}