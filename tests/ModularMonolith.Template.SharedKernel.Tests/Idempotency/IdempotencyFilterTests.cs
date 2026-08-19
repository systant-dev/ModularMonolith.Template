using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ModularMonolith.Template.SharedKernel.Idempotency;
using Moq;

namespace ModularMonolith.Template.SharedKernel.Tests.Idempotency;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Http)]
public sealed class IdempotencyFilterTests
{
    #region Test Method Declarations

    [Fact]
    public async Task InvokeAsync_WhenKeyMissing_ReturnsBadRequestAndDoesNotInvokeNext()
    {
        bool nextInvoked = false;
        ValueTask<object?> next(EndpointFilterInvocationContext _)
        {
            nextInvoked = true;
            return ValueTask.FromResult<object?>(null);
        };

        Mock<IIdempotencyStore> store = new(MockBehavior.Strict);
        IdempotencyFilter filter = CreateFilter(store.Object);
        EndpointFilterInvocationContext context = CreateContext(idempotencyKey: null);

        object? result = await filter.InvokeAsync(context, next);

        nextInvoked.Should().BeFalse();
        result.Should().BeOfType<ProblemHttpResult>()
              .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task InvokeAsync_WhenKeyReserved_InvokesNext()
    {
        object sentinel = new();
        bool nextInvoked = false;
        ValueTask<object?> next(EndpointFilterInvocationContext _)
        {
            nextInvoked = true;
            return ValueTask.FromResult<object?>(sentinel);
        }

        Mock<IIdempotencyStore> store = new();
        store.Setup(s => s.TryReserveAsync("key-1", It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);
        IdempotencyFilter filter = CreateFilter(store.Object);
        EndpointFilterInvocationContext context = CreateContext("key-1");

        object? result = await filter.InvokeAsync(context, next);

        nextInvoked.Should().BeTrue();
        result.Should().BeSameAs(sentinel);
    }

    [Fact]
    public async Task InvokeAsync_WhenKeyAlreadySeen_ReturnsConflictAndDoesNotInvokeNext()
    {
        bool nextInvoked = false;
        ValueTask<object?> next(EndpointFilterInvocationContext _)
        {
            nextInvoked = true;
            return ValueTask.FromResult<object?>(null);
        }

        Mock<IIdempotencyStore> store = new();
        store.Setup(s => s.TryReserveAsync("key-1", It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
        IdempotencyFilter filter = CreateFilter(store.Object);
        EndpointFilterInvocationContext context = CreateContext("key-1");

        object? result = await filter.InvokeAsync(context, next);

        nextInvoked.Should().BeFalse();
        result.Should().BeOfType<ProblemHttpResult>()
              .Which.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    #endregion

    #region Private Method Declarations

    private static IdempotencyFilter CreateFilter(IIdempotencyStore store)
    {
        IdempotencyFilter filter = new(store, NullLogger<IdempotencyFilter>.Instance);
        return filter;
    }

    private static EndpointFilterInvocationContext CreateContext(string? idempotencyKey)
    {
        DefaultHttpContext httpContext = new();
        if (idempotencyKey is not null)
        {
            httpContext.Request.Headers[IdempotencyFilter.IdempotencyKeyHeaderName] = idempotencyKey;
        }

        return EndpointFilterInvocationContext.Create(httpContext);
    }

    #endregion
}
