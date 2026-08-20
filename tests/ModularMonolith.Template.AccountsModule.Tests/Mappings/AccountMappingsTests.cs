using AccountsModule.Contracts.Requests;
using AccountsModule.Contracts.Responses;
using AccountsModule.Mappings;
using AccountsModule.Models;
using FluentAssertions;

namespace ModularMonolith.Template.AccountsModule.Tests.Mappings;

[Component(TestComponent.AccountsModule)]
[Category(TestCategory.Modular)]
public sealed class AccountMappingsTests
{
    #region Test Method Declarations

    [Fact]
    public void ToResponse_MapsAllProperties()
    {
        Account account = new()
        {
            Id = 3,
            UserId = 7,
            AccountNumber = "ACC-3",
            AccountHolderName = "Ada Lovelace",
            Balance = 125.50m
        };

        AccountResponse response = account.ToResponse();

        response.Id.Should().Be(3);
        response.UserId.Should().Be(7);
        response.AccountNumber.Should().Be("ACC-3");
        response.AccountHolderName.Should().Be("Ada Lovelace");
        response.Balance.Should().Be(125.50m);
    }

    [Fact]
    public void CreateAccountRequest_ToAccount_MapsMatchingProperties()
    {
        CreateAccountRequest request = new()
        {
            UserId = 42,
            AccountHolderName = "Grace Hopper",
            InitialDeposit = 50m
        };

        Account account = request.ToAccount();

        account.UserId.Should().Be(42);
        account.AccountHolderName.Should().Be("Grace Hopper");
    }

    #endregion
}
