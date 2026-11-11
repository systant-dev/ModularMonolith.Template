using FluentAssertions;
using ModularMonolith.Template.SharedKernel.Results;

namespace ModularMonolith.Template.SharedKernel.Tests.Results;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Results)]
public sealed class ResultTests
{
    #region Test Method Declarations

    [Fact]
    public void Success_CreatesSuccessfulResult()
    {
        Result result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_CreatesFailedResult()
    {
        Error error = Error.NotFound("user.not-found", "User was not found.");

        Result result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Failure_WithNoneError_Throws()
    {
        Action act = () => Result.Failure(Error.None);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenericSuccess_ExposesValue()
    {
        Result<int> result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void GenericFailure_AccessingValue_Throws()
    {
        Result<int> result = Result.Failure<int>(Error.Failure("code", "message"));

        result.IsFailure.Should().BeTrue();

        Func<int> act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccess()
    {
        Result<string> result = "hello";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailure()
    {
        Error error = Error.Validation("name.required", "Name is required.");

        Result<string> result = error;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    #endregion
}
