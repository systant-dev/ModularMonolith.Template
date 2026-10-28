using FluentAssertions;
using ModularMonolith.Template.SharedKernel.Results;

namespace ModularMonolith.Template.SharedKernel.Tests.Results;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Results)]
public sealed class ErrorTests
{
    #region Test Method Declarations

    [Fact]
    public void None_HasFailureTypeAndEmptyValues()
    {
        Error.None.Code.Should().BeEmpty();
        Error.None.Message.Should().BeEmpty();
        Error.None.Type.Should().Be(ErrorType.Failure);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Unauthorised)]
    [InlineData(ErrorType.Forbidden)]
    [InlineData(ErrorType.Failure)]
    public void Factories_SetExpectedType(ErrorType type)
    {
        Error error = type switch
        {
            ErrorType.Validation => Error.Validation("code", "message"),
            ErrorType.NotFound => Error.NotFound("code", "message"),
            ErrorType.Conflict => Error.Conflict("code", "message"),
            ErrorType.Unauthorised => Error.Unauthorised("code", "message"),
            ErrorType.Forbidden => Error.Forbidden("code", "message"),
            _ => Error.Failure("code", "message")
        };

        error.Code.Should().Be("code");
        error.Message.Should().Be("message");
        error.Type.Should().Be(type);
    }

    [Fact]
    public void Constructor_WithNullCode_Throws()
    {
        Action act = () => _ = new Error(null!, "message", ErrorType.Failure);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Errors_WithSameValues_AreEqual()
    {
        Error first = Error.Failure("code", "message");
        Error second = Error.Failure("code", "message");

        first.Should().Be(second);
    }

    #endregion
}
