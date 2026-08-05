using FluentAssertions;
using UserModule.Models;
using UserModule.Storage;

namespace ModularMonolith.Template.UserModule.Tests.Storage;

[Component(TestComponent.UserModule)]
[Category(TestCategory.Modular)]
public sealed class UserStoreTests
{
    #region Test Method Declarations

    [Fact]
    public void GetAll_WhenEmpty_ReturnsEmptyCollection()
    {
        UserStore store = new();

        store.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void Add_AssignsIncrementingIds()
    {
        UserStore store = new();

        User first = store.Add(NewUser("Ada Lovelace", "ada@example.com"));
        User second = store.Add(NewUser("Alan Turing", "alan@example.com"));

        first.Id.Should().Be(1);
        second.Id.Should().Be(2);
    }

    [Fact]
    public void Add_DoesNotMutateSuppliedInstance()
    {
        UserStore store = new();
        User original = NewUser("Grace Hopper", "grace@example.com");

        User created = store.Add(original);

        original.Id.Should().Be(0);
        created.Id.Should().Be(1);
        created.Should().NotBeSameAs(original);
    }

    [Fact]
    public void GetAll_ReturnsUsersOrderedById()
    {
        UserStore store = new();
        store.Add(NewUser("First", "first@example.com"));
        store.Add(NewUser("Second", "second@example.com"));
        store.Add(NewUser("Third", "third@example.com"));

        IReadOnlyCollection<User> all = store.GetAll();

        all.Select(user => user.Id).Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public void Get_WhenExists_ReturnsUser()
    {
        UserStore store = new();
        User created = store.Add(NewUser("Katherine Johnson", "katherine@example.com"));

        User? found = store.Get(created.Id);

        found.Should().NotBeNull();
        found!.Id.Should().Be(created.Id);
    }

    [Fact]
    public void Get_WhenMissing_ReturnsNull()
    {
        UserStore store = new();

        store.Get(999).Should().BeNull();
    }

    [Fact]
    public void Update_WhenExists_ReplacesUserAndPreservesId()
    {
        UserStore store = new();
        User created = store.Add(NewUser("Original", "original@example.com"));

        User? updated = store.Update(created.Id, NewUser("Updated", "updated@example.com", "555-0199"));

        updated.Should().NotBeNull();
        updated!.Id.Should().Be(created.Id);
        updated.FullName.Should().Be("Updated");
        updated.Email.Should().Be("updated@example.com");
        updated.PhoneNumber.Should().Be("555-0199");
        store.Get(created.Id)!.FullName.Should().Be("Updated");
    }

    [Fact]
    public void Update_WhenMissing_ReturnsNull()
    {
        UserStore store = new();

        store.Update(999, NewUser("Nobody", "nobody@example.com")).Should().BeNull();
    }

    [Fact]
    public void Remove_WhenExists_ReturnsTrueAndRemoves()
    {
        UserStore store = new();
        User created = store.Add(NewUser("Temp", "temp@example.com"));

        store.Remove(created.Id).Should().BeTrue();
        store.Get(created.Id).Should().BeNull();
    }

    [Fact]
    public void Remove_WhenMissing_ReturnsFalse()
    {
        UserStore store = new();

        store.Remove(999).Should().BeFalse();
    }

    #endregion

    #region Private Method Declarations

    private static User NewUser(string fullName, string email, string? phoneNumber = null) =>
        new()
        {
            FullName = fullName,
            Email = email,
            PhoneNumber = phoneNumber
        };

    #endregion
}
