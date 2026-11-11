using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ModularMonolith.Template.SharedKernel.Results;

namespace ModularMonolith.Template.SharedKernel.Tests.Results;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Results)]
public sealed class ErrorHttpExtensionsTests
{
    #region Test Method Declarations

    [Theory]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.Unauthorised, StatusCodes.Status401Unauthorized)]
    [InlineData(ErrorType.Forbidden, StatusCodes.Status403Forbidden)]
    [InlineData(ErrorType.Failure, StatusCodes.Status500InternalServerError)]
    public void ToProblem_MapsErrorTypeToStatusCode(ErrorType type, int expectedStatusCode)
    {
        Error error = new("code", "message", type);

        ProblemHttpResult result = error.ToProblem();

        result.StatusCode.Should().Be(expectedStatusCode);
        result.ProblemDetails.Title.Should().Be("code");
        result.ProblemDetails.Detail.Should().Be("message");
    }

    #endregion
}
