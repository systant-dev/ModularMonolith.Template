using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging.Abstractions;
using ModularMonolith.Template.SharedKernel.Validation;

namespace ModularMonolith.Template.SharedKernel.Tests.Validation;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Validation)]
public sealed class ValidationFilterTests
{
    #region Test Method Declarations

    [Fact]
    public async Task InvokeAsync_WhenValid_InvokesNext()
    {
        object sentinel = new();
        bool nextInvoked = false;
        EndpointFilterDelegate next = _ =>
        {
            nextInvoked = true;
            return ValueTask.FromResult<object?>(sentinel);
        };

        ValidationFilter<TestRequest> filter = CreateFilter(NotEmptyNameValidator());
        TestRequest request = new() { Name = "valid" };
        EndpointFilterInvocationContext context = CreateContext(request);

        object? result = await filter.InvokeAsync(context, next);

        nextInvoked.Should().BeTrue();
        result.Should().BeSameAs(sentinel);
    }
    [Fact]
    public async Task InvokeAsync_WhenInvalid_ReturnsValidationProblemAndDoesNotInvokeNext()
    {
        bool nextInvoked = false;
        EndpointFilterDelegate next = _ =>
        {
            nextInvoked = true;
            return ValueTask.FromResult<object?>(null);
        };

        ValidationFilter<TestRequest> filter = CreateFilter(NotEmptyNameValidator());
        TestRequest request = new() { Name = string.Empty };
        EndpointFilterInvocationContext context = CreateContext(request);

        object? result = await filter.InvokeAsync(context, next);

        nextInvoked.Should().BeFalse();
        result.Should().BeOfType<ValidationProblem>()
            .Which.ProblemDetails.Errors.Should().ContainKey(nameof(TestRequest.Name));
    }

    [Fact]
    public async Task InvokeAsync_WhenNoMatchingArgument_InvokesNextWithoutValidating()
    {
        object sentinel = new();
        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(sentinel);

        // The validator would fail any TestRequest, so a sentinel result proves validation was skipped.
        ValidationFilter<TestRequest> filter = CreateFilter(NotEmptyNameValidator());
        EndpointFilterInvocationContext context = CreateContext("not-a-request");

        object? result = await filter.InvokeAsync(context, next);

        result.Should().BeSameAs(sentinel);
    }

    #endregion

    #region Private Method Declarations

    private static ValidationFilter<TestRequest> CreateFilter(IValidator<TestRequest> validator)
    {
        ValidationFilter<TestRequest> filter = new(validator, NullLogger<ValidationFilter<TestRequest>>.Instance);
        return filter;
    }

    private static InlineValidator<TestRequest> NotEmptyNameValidator()
    {
        InlineValidator<TestRequest> validator = [];
        validator.RuleFor(request => request.Name).NotEmpty();
        return validator;
    }

    private static EndpointFilterInvocationContext CreateContext(object argument)
    {
        DefaultHttpContext httpContext = new();
        return EndpointFilterInvocationContext.Create(httpContext, argument);
    }

    #endregion

    #region Nested Type Declarations

    public sealed class TestRequest
    {
        public string Name { get; init; } = string.Empty;
    }

    #endregion
}
