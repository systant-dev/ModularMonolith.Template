using FluentValidation.TestHelper;
using UserModule.Contracts.Requests;
using UserModule.Validation;

namespace ModularMonolith.Template.UserModule.Tests.Validation;

[Component(TestComponent.UserModule)]
[Category(TestCategory.Validation)]
public sealed class UpdateUserRequestValidatorTests
{
    #region Field Declarations

    private readonly UpdateUserRequestValidator _validator = new();

    #endregion

    #region Test Method Declarations

    [Fact]
    public void Validate_WithValidRequest_HasNoErrors()
    {
        UpdateUserRequest request = NewRequest();

        TestValidationResult<UpdateUserRequest> result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNullPhoneNumber_HasNoErrors()
    {
        UpdateUserRequest request = NewRequest() with { PhoneNumber = null };

        TestValidationResult<UpdateUserRequest> result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithEmptyFullName_HasError(string fullName)
    {
        UpdateUserRequest request = NewRequest() with { FullName = fullName };

        TestValidationResult<UpdateUserRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(request => request.FullName);
    }

    [Fact]
    public void Validate_WithFullNameExceedingMaxLength_HasError()
    {
        UpdateUserRequest request = NewRequest() with { FullName = new string('a', 201) };

        TestValidationResult<UpdateUserRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(request => request.FullName);
    }

    [Fact]
    public void Validate_WithEmptyEmail_HasError()
    {
        UpdateUserRequest request = NewRequest() with { Email = "" };

        TestValidationResult<UpdateUserRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(request => request.Email);
    }

    [Fact]
    public void Validate_WithInvalidEmail_HasError()
    {
        UpdateUserRequest request = NewRequest() with { Email = "not-an-email" };

        TestValidationResult<UpdateUserRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(request => request.Email);
    }

    [Fact]
    public void Validate_WithEmailExceedingMaxLength_HasError()
    {
        UpdateUserRequest request = NewRequest() with { Email = $"{new string('a', 320)}@example.com" };

        TestValidationResult<UpdateUserRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(request => request.Email);
    }

    [Fact]
    public void Validate_WithPhoneNumberExceedingMaxLength_HasError()
    {
        UpdateUserRequest request = NewRequest() with { PhoneNumber = new string('1', 31) };

        TestValidationResult<UpdateUserRequest> result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(request => request.PhoneNumber);
    }

    #endregion

    #region Private Method Declarations

    private static UpdateUserRequest NewRequest() =>
        new()
        {
            FullName = "Ada Lovelace",
            Email = "ada@example.com",
            PhoneNumber = "555-0100"
        };

    #endregion
}
