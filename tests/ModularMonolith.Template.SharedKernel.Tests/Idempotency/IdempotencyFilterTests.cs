using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ModularMonolith.Template.SharedKernel.Idempotency;
using Moq;
using System.Security.Claims;

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
        store.Setup(s => s.TryReserveAsync(It.Is<string>(key => key.EndsWith("|key-1", StringComparison.Ordinal)), It.IsAny<CancellationToken>()))
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
        store.Setup(s => s.TryReserveAsync(It.Is<string>(key => key.EndsWith("|key-1", StringComparison.Ordinal)), It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
        IdempotencyFilter filter = CreateFilter(store.Object);
        EndpointFilterInvocationContext context = CreateContext("key-1");

        object? result = await filter.InvokeAsync(context, next);

        nextInvoked.Should().BeFalse();
        result.Should().BeOfType<ProblemHttpResult>()
              .Which.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task InvokeAsync_WhenKeyTooLong_ReturnsBadRequestAndDoesNotReserve()
    {
        static ValueTask<object?> next(EndpointFilterInvocationContext _) => ValueTask.FromResult<object?>(null);

        Mock<IIdempotencyStore> store = new(MockBehavior.Strict);
        IdempotencyFilter filter = CreateFilter(store.Object);
        EndpointFilterInvocationContext context = CreateContext(new string('k', IdempotencyFilter.MaxKeyLength + 1));

        object? result = await filter.InvokeAsync(context, next);

        result.Should().BeOfType<ProblemHttpResult>()
              .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task InvokeAsync_ReservesTheKeyPerCallerAndRoute()
    {
        static ValueTask<object?> next(EndpointFilterInvocationContext _) => ValueTask.FromResult<object?>(null);

        List<string> reservedKeys = [];
        Mock<IIdempotencyStore> store = new();
        store.Setup(s => s.TryReserveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .Callback((string key, CancellationToken _) => reservedKeys.Add(key))
             .ReturnsAsync(true);
        IdempotencyFilter filter = CreateFilter(store.Object);

        await filter.InvokeAsync(CreateContext("key-1", user: "alice", path: "/v1/users"), next);
        await filter.InvokeAsync(CreateContext("key-1", user: "bob", path: "/v1/users"), next);
        await filter.InvokeAsync(CreateContext("key-1", user: "alice", path: "/v1/accounts"), next);
        await filter.InvokeAsync(CreateContext("key-1", user: "alice", path: "/v1/users"), next);

        reservedKeys.Should().HaveCount(4);
        reservedKeys.Distinct().Should().HaveCount(3);
        reservedKeys[3].Should().Be(reservedKeys[0]);
    }

    #endregion

    #region Private Method Declarations

    private static IdempotencyFilter CreateFilter(IIdempotencyStore store)
    {
        IdempotencyFilter filter = new(store, NullLogger<IdempotencyFilter>.Instance);
        return filter;
    }

    private static EndpointFilterInvocationContext CreateContext(string? idempotencyKey, string? user = null, string path = "/")
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.Path = path;
        if (user is not null)
        {
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user)], "Test"));
        }

        if (idempotencyKey is not null)
        {
            httpContext.Request.Headers[IdempotencyFilter.IdempotencyKeyHeaderName] = idempotencyKey;
        }

        return EndpointFilterInvocationContext.Create(httpContext);
    }

    #endregion
}
