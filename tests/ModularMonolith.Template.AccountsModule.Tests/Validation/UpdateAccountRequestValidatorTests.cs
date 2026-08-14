using AccountsModule.Contracts.Requests;
using AccountsModule.Validation;
using FluentValidation.TestHelper;

namespace ModularMonolith.Template.AccountsModule.Tests.Validation;

[Component(TestComponent.AccountsModule)]
[Category(TestCategory.Validation)]
public sealed class UpdateAccountRequestValidatorTests
{
    #region Field Declarations

    private readonly UpdateAccountRequestValidator _validator = new();

    #endregion

    #region Test Method Declarations

    [Fact]
    public void Validate_WithValidRequest_HasNoErrors()
    {
        UpdateAccountRequest request = NewRequest();

        TestValidationResult<UpdateAccountRequest> result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithEmptyAccountHolderName_HasError(string accountHolderName)
    {
        UpdateAccountRequest request = NewRequest() with { AccountHolderName = accountHolderName };

        TestValidationResult<UpdateAccountRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(request => request.AccountHolderName);
    }

    [Fact]
    public void Validate_WithAccountHolderNameExceedingMaxLength_HasError()
    {
        UpdateAccountRequest request = NewRequest() with { AccountHolderName = new string('a', 201) };

        TestValidationResult<UpdateAccountRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(request => request.AccountHolderName);
    }

    #endregion

    #region Private Method Declarations

    private static UpdateAccountRequest NewRequest()
    {
        UpdateAccountRequest request = new()
        {
            AccountHolderName = "Ada Lovelace"
        };

        return request;
    }

    #endregion
}
