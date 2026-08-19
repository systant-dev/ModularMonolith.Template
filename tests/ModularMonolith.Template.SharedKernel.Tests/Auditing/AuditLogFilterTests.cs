using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Testing;
using ModularMonolith.Template.SharedKernel.Auditing;

namespace ModularMonolith.Template.SharedKernel.Tests.Auditing;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Http)]
public sealed class AuditLogFilterTests
{
    #region Test Method Declarations

    [Fact]
    public async Task InvokeAsync_InvokesNextAndReturnsResult()
    {
        object sentinel = new();
        bool nextInvoked = false;
        ValueTask<object?> next(EndpointFilterInvocationContext _)
        {
            nextInvoked = true;
            return ValueTask.FromResult<object?>(sentinel);
        };

        FakeLogger<AuditLogFilter> logger = new();
        AuditLogFilter filter = new(logger);
        EndpointFilterInvocationContext context = CreateContext();

        object? result = await filter.InvokeAsync(context, next);

        nextInvoked.Should().BeTrue();
        result.Should().BeSameAs(sentinel);
    }

    [Fact]
    public async Task InvokeAsync_WritesAuditLogRecord()
    {
        static ValueTask<object?> next(EndpointFilterInvocationContext _) => ValueTask.FromResult<object?>(null);

        FakeLogger<AuditLogFilter> logger = new();
        AuditLogFilter filter = new(logger);
        EndpointFilterInvocationContext context = CreateContext();

        await filter.InvokeAsync(context, next);

        logger.Collector.Count.Should().Be(1);
        logger.LatestRecord.Message.Should().Contain("POST").And.Contain("/v1/accounts");
    }

    #endregion

    #region Private Method Declarations

    private static EndpointFilterInvocationContext CreateContext()
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.Path = "/v1/accounts";
        return EndpointFilterInvocationContext.Create(httpContext);
    }

    #endregion
}
