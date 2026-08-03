using FluentAssertions;
using System.Collections.Immutable;

namespace ModularMonolith.Template.SourceGenerators.Shared.Tests.Collections;

[Component(TestComponent.SourceGeneratorsShared)]
[Category(TestCategory.Cacheability)]
public sealed class EquatableArrayTests
{
    #region Equality

    [Fact]
    public void Equals_ReturnsTrue_ForSameElementsInSameOrder()
    {
        EquatableArray<int> left = new([1, 2, 3]);
        EquatableArray<int> right = new([1, 2, 3]);

        left.Equals(right).Should().BeTrue();
    }

    [Fact]
    public void Equals_ReturnsFalse_ForDifferentLengths()
    {
        EquatableArray<int> left = new([1, 2, 3]);
        EquatableArray<int> right = new([1, 2]);

        left.Equals(right).Should().BeFalse();
    }

    [Fact]
    public void Equals_ReturnsFalse_ForSameElementsInDifferentOrder()
    {
        EquatableArray<int> left = new([1, 2, 3]);
        EquatableArray<int> right = new([3, 2, 1]);

        left.Equals(right).Should().BeFalse();
    }

    [Fact]
    public void Equals_Object_ReturnsFalse_ForDifferentType()
    {
        EquatableArray<int> array = new([1, 2, 3]);

        array.Equals("not an array").Should().BeFalse();
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenElementsDiffer()
    {
        EquatableArray<int> left = new([1, 2, 3]);
        EquatableArray<int> right = new([1, 2, 4]);

        left.Equals(right).Should().BeFalse();
    }

    #endregion

    #region Hashing

    [Fact]
    public void GetHashCode_IsEqual_ForEqualArrays()
    {
        EquatableArray<int> left = new([1, 2, 3]);
        EquatableArray<int> right = new([1, 2, 3]);

        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void GetHashCode_IsOrderSensitive()
    {
        EquatableArray<int> left = new([1, 2, 3]);
        EquatableArray<int> right = new([3, 2, 1]);

        left.GetHashCode().Should().NotBe(right.GetHashCode());
    }

    #endregion

    #region Default And Empty

    [Fact]
    public void Empty_HasZeroLength()
    {
        EquatableArray<int>.Empty.Length.Should().Be(0);
    }

    [Fact]
    public void DefaultArray_BehavesAsEmpty()
    {
        EquatableArray<int> defaultArray = default;

        defaultArray.Length.Should().Be(0);
        defaultArray.Should().BeEmpty();
        defaultArray.Equals(EquatableArray<int>.Empty).Should().BeTrue();
    }

    #endregion

    #region Indexing And Enumeration

    [Fact]
    public void Indexer_ReturnsElementAtPosition()
    {
        EquatableArray<int> array = new([10, 20, 30]);

        array[1].Should().Be(20);
    }

    [Fact]
    public void Enumeration_YieldsAllElementsInOrder()
    {
        EquatableArray<int> array = new([10, 20, 30]);

        array.Should().Equal(10, 20, 30);
    }

    #endregion

    #region Conversion

    [Fact]
    public void ImplicitConversion_FromImmutableArray_PreservesElements()
    {
        ImmutableArray<int> source = [1, 2, 3];

        EquatableArray<int> array = source;

        array.Length.Should().Be(3);
        array.Should().Equal(1, 2, 3);
    }

    #endregion
}
