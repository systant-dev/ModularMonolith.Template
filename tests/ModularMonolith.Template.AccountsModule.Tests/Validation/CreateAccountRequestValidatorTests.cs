using AccountsModule.Contracts.Requests;
using AccountsModule.Validation;
using FluentValidation.TestHelper;

namespace ModularMonolith.Template.AccountsModule.Tests.Validation;

[Component(TestComponent.AccountsModule)]
[Category(TestCategory.Validation)]
public sealed class CreateAccountRequestValidatorTests
{
    #region Field Declarations

    private readonly CreateAccountRequestValidator _validator = new();

    #endregion

    #region Test Method Declarations

    [Fact]
    public void Validate_WithValidRequest_HasNoErrors()
    {
        CreateAccountRequest request = NewRequest();

        TestValidationResult<CreateAccountRequest> result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithNonPositiveUserId_HasError(int userId)
    {
        CreateAccountRequest request = NewRequest() with { UserId = userId };

        TestValidationResult<CreateAccountRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(request => request.UserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithEmptyAccountHolderName_HasError(string accountHolderName)
    {
        CreateAccountRequest request = NewRequest() with { AccountHolderName = accountHolderName };

        TestValidationResult<CreateAccountRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(request => request.AccountHolderName);
    }

    [Fact]
    public void Validate_WithAccountHolderNameExceedingMaxLength_HasError()
    {
        CreateAccountRequest request = NewRequest() with { AccountHolderName = new string('a', 201) };

        TestValidationResult<CreateAccountRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(request => request.AccountHolderName);
    }

    [Fact]
    public void Validate_WithNegativeInitialDeposit_HasError()
    {
        CreateAccountRequest request = NewRequest() with { InitialDeposit = -1m };

        TestValidationResult<CreateAccountRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(request => request.InitialDeposit);
    }

    #endregion

    #region Private Method Declarations

    private static CreateAccountRequest NewRequest() => new()
    {
        UserId = 1,
        AccountHolderName = "Ada Lovelace",
        InitialDeposit = 100m
    };

    #endregion
}
