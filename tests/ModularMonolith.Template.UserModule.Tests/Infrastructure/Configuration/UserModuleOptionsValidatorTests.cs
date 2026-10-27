using FluentValidation.TestHelper;
using UserModule.Infrastructure.Configuration;

namespace ModularMonolith.Template.UserModule.Tests.Infrastructure.Configuration;

[Component(TestComponent.UserModule)]
[Category(TestCategory.Configuration)]
public sealed class UserModuleOptionsValidatorTests
{
    #region Field Declarations

    private readonly UserModuleOptionsValidator _validator = new();

    #endregion

    #region Test Method Declarations

    [Fact]
    public void Validate_WithValidOptions_HasNoErrors()
    {
        UserModuleOptions options = NewOptions();

        TestValidationResult<UserModuleOptions> result = _validator.TestValidate(options);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithNonPositiveMaxUsers_HasError(int maxUsers)
    {
        UserModuleOptions options = NewOptions() with { MaxUsers = maxUsers };

        TestValidationResult<UserModuleOptions> result = _validator.TestValidate(options);

        result.ShouldHaveValidationErrorFor(options => options.MaxUsers);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithEmptyConnectionString_HasError(string connectionString)
    {
        UserModuleOptions options = NewOptions() with { ConnectionString = connectionString };

        TestValidationResult<UserModuleOptions> result = _validator.TestValidate(options);

        result.ShouldHaveValidationErrorFor(options => options.ConnectionString);
    }

    #endregion

    #region Private Method Declarations

    private static UserModuleOptions NewOptions()
    {
        UserModuleOptions options = new()
        {
            Enabled = true,
            MaxUsers = 1000,
            ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=ModularMonolith;Trusted_Connection=True"
        };

        return options;
    }

    #endregion
}
