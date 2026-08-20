using FluentAssertions;
using UserModule.Contracts.Requests;
using UserModule.Contracts.Responses;
using UserModule.Mappings;
using UserModule.Models;

namespace ModularMonolith.Template.UserModule.Tests.Mappings;

[Component(TestComponent.UserModule)]
[Category(TestCategory.Modular)]
public sealed class UserMappingsTests
{
    #region Test Method Declarations

    [Fact]
    public void ToResponse_MapsAllProperties()
    {
        User user = new()
        {
            Id = 7,
            FullName = "Ada Lovelace",
            Email = "ada@example.com",
            PhoneNumber = "555-0100"
        };

        UserResponse response = user.ToResponse();

        response.Id.Should().Be(7);
        response.FullName.Should().Be("Ada Lovelace");
        response.Email.Should().Be("ada@example.com");
        response.PhoneNumber.Should().Be("555-0100");
    }

    [Fact]
    public void CreateUserRequest_ToUser_MapsAllProperties()
    {
        CreateUserRequest request = new()
        {
            FullName = "Grace Hopper",
            Email = "grace@example.com",
            PhoneNumber = "555-0111"
        };

        User user = request.ToUser();

        user.FullName.Should().Be("Grace Hopper");
        user.Email.Should().Be("grace@example.com");
        user.PhoneNumber.Should().Be("555-0111");
    }

    [Fact]
    public void UpdateUserRequest_ToUser_MapsAllProperties()
    {
        UpdateUserRequest request = new()
        {
            FullName = "Alan Turing",
            Email = "alan@example.com",
            PhoneNumber = null
        };

        User user = request.ToUser();

        user.FullName.Should().Be("Alan Turing");
        user.Email.Should().Be("alan@example.com");
        user.PhoneNumber.Should().BeNull();
    }

    #endregion
}
