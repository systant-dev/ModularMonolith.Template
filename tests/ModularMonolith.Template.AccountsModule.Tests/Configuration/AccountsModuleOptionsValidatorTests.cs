using FluentValidation.TestHelper;
using AccountsModule.Configuration;

namespace ModularMonolith.Template.AccountsModule.Tests.Configuration;

[Component(TestComponent.AccountsModule)]
[Category(TestCategory.Configuration)]
public sealed class AccountsModuleOptionsValidatorTests
{
    #region Field Declarations

    private readonly AccountsModuleOptionsValidator _validator = new();

    #endregion

    #region Test Method Declarations

    [Fact]
    public void Validate_WithValidOptions_HasNoErrors()
    {
        AccountsModuleOptions options = NewOptions();

        TestValidationResult<AccountsModuleOptions> result = _validator.TestValidate(options);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithNonPositiveMaxAccounts_HasError(int maxAccounts)
    {
        AccountsModuleOptions options = NewOptions() with { MaxAccounts = maxAccounts };

        TestValidationResult<AccountsModuleOptions> result = _validator.TestValidate(options);

        result.ShouldHaveValidationErrorFor(options => options.MaxAccounts);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithEmptyConnectionString_HasError(string connectionString)
    {
        AccountsModuleOptions options = NewOptions() with { ConnectionString = connectionString };

        TestValidationResult<AccountsModuleOptions> result = _validator.TestValidate(options);

        result.ShouldHaveValidationErrorFor(options => options.ConnectionString);
    }

    #endregion

    #region Private Method Declarations

    private static AccountsModuleOptions NewOptions() => new()
    {
        Enabled = true,
        MaxAccounts = 1000,
        ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=ModularMonolithAccounts;Trusted_Connection=True"
    };

    #endregion
}
