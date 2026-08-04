using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using UserModule.Contracts.Requests;
using UserModule.Contracts.Responses;
using UserModule.Services;
using UserModule.Storage;

namespace ModularMonolith.Template.UserModule.Tests.Services;

[Component(TestComponent.UserModule)]
[Category(TestCategory.Modular)]
public sealed class UserServiceTests
{
    #region Test Method Declarations

    [Fact]
    public void GetAll_WhenEmpty_ReturnsEmptyCollection()
    {
        UserService service = CreateService();

        service.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void GetAll_ReturnsCreatedUsers()
    {
        UserService service = CreateService();
        service.Create(NewRequest("Ada Lovelace", "ada@example.com"));
        service.Create(NewRequest("Alan Turing", "alan@example.com"));

        IReadOnlyCollection<UserResponse> all = service.GetAll();

        all.Should().HaveCount(2);
        all.Select(user => user.FullName).Should().Contain(["Ada Lovelace", "Alan Turing"]);
    }

    [Fact]
    public void Create_AssignsIdAndMapsFields()
    {
        UserService service = CreateService();

        UserResponse created = service.Create(NewRequest("Grace Hopper", "grace@example.com", "555-0100"));

        created.Id.Should().BePositive();
        created.FullName.Should().Be("Grace Hopper");
        created.Email.Should().Be("grace@example.com");
        created.PhoneNumber.Should().Be("555-0100");
    }

    [Fact]
    public void Find_WhenExists_ReturnsUser()
    {
        UserService service = CreateService();
        UserResponse created = service.Create(NewRequest("Katherine Johnson", "katherine@example.com"));

        UserResponse? found = service.Find(created.Id);

        found.Should().NotBeNull();
        found!.Id.Should().Be(created.Id);
    }

    [Fact]
    public void Find_WhenMissing_ReturnsNull()
    {
        UserService service = CreateService();

        service.Find(999).Should().BeNull();
    }

    [Fact]
    public void Update_WhenExists_ReturnsUpdatedUser()
    {
        UserService service = CreateService();
        UserResponse created = service.Create(NewRequest("Original Name", "original@example.com"));

        UserResponse? updated = service.Update(created.Id, new UpdateUserRequest
        {
            FullName = "Updated Name",
            Email = "updated@example.com",
            PhoneNumber = "555-0199"
        });

        updated.Should().NotBeNull();
        updated!.Id.Should().Be(created.Id);
        updated.FullName.Should().Be("Updated Name");
        updated.Email.Should().Be("updated@example.com");
        updated.PhoneNumber.Should().Be("555-0199");
    }

    [Fact]
    public void Update_WhenMissing_ReturnsNull()
    {
        UserService service = CreateService();

        UserResponse? updated = service.Update(999, new UpdateUserRequest
        {
            FullName = "Nobody",
            Email = "nobody@example.com"
        });

        updated.Should().BeNull();
    }

    [Fact]
    public void Delete_WhenExists_ReturnsTrueAndRemoves()
    {
        UserService service = CreateService();
        UserResponse created = service.Create(NewRequest("Temp User", "temp@example.com"));

        service.Delete(created.Id).Should().BeTrue();
        service.Find(created.Id).Should().BeNull();
    }

    [Fact]
    public void Delete_WhenMissing_ReturnsFalse()
    {
        UserService service = CreateService();

        service.Delete(999).Should().BeFalse();
    }

    #endregion

    #region Private Method Declarations

    private static UserService CreateService() =>
        new(new UserStore(), NullLogger<UserService>.Instance);

    private static CreateUserRequest NewRequest(string fullName, string email, string? phoneNumber = null) =>
        new()
        {
            FullName = fullName,
            Email = email,
            PhoneNumber = phoneNumber
        };

    #endregion
}
