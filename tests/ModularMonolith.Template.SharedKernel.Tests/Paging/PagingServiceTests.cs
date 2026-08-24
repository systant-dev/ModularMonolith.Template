using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ModularMonolith.Template.SharedKernel.Paging;

namespace ModularMonolith.Template.SharedKernel.Tests.Paging;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Http)]
public sealed class PagingServiceTests
{
    #region Field Declarations

    private static readonly IReadOnlyCollection<int> SamplePage = [1, 2, 3];

    #endregion

    #region Test Method Declarations

    [Fact]
    public async Task GetPageAsync_ClampsLimitToConfiguredMaximum()
    {
        PagingService service = CreateService(new PagingOptions { DefaultLimit = 20, MaxLimit = 50 });
        HttpResponse response = new DefaultHttpContext().Response;
        int observedLimit = -1;

        await service.GetPageAsync(
            new PageRequest(AfterId: 0, Limit: 1000),
            response,
            (afterId, limit, _) =>
            {
                observedLimit = limit;
                return Task.FromResult(SamplePage);
            },
            _ => Task.FromResult(0),
            TestContext.Current.CancellationToken);

        observedLimit.Should().Be(50);
    }

    [Fact]
    public async Task GetPageAsync_UsesDefaultLimit_WhenLimitIsNull()
    {
        PagingService service = CreateService(new PagingOptions { DefaultLimit = 25, MaxLimit = 100 });
        HttpResponse response = new DefaultHttpContext().Response;
        int observedLimit = -1;

        await service.GetPageAsync(
            new PageRequest(AfterId: null, Limit: null),
            response,
            (afterId, limit, _) =>
            {
                observedLimit = limit;
                return Task.FromResult(SamplePage);
            },
            _ => Task.FromResult(0),
            TestContext.Current.CancellationToken);

        observedLimit.Should().Be(25);
    }

    [Fact]
    public async Task GetPageAsync_ClampsNegativeCursorToZero()
    {
        PagingService service = CreateService(new PagingOptions());
        HttpResponse response = new DefaultHttpContext().Response;
        int observedAfterId = -1;

        await service.GetPageAsync(
            new PageRequest(AfterId: -5, Limit: 10),
            response,
            (afterId, limit, _) =>
            {
                observedAfterId = afterId;
                return Task.FromResult(SamplePage);
            },
            _ => Task.FromResult(0),
            TestContext.Current.CancellationToken);

        observedAfterId.Should().Be(0);
    }

    [Fact]
    public async Task GetPageAsync_WhenIncludeCount_WritesTotalCountHeader()
    {
        PagingService service = CreateService(new PagingOptions());
        HttpResponse response = new DefaultHttpContext().Response;

        await service.GetPageAsync(
            new PageRequest(AfterId: 0, Limit: 10, IncludeCount: true),
            response,
            (afterId, limit, _) => Task.FromResult(SamplePage),
            _ => Task.FromResult(42),
            TestContext.Current.CancellationToken);

        response.Headers[PagingService.TotalCountHeaderName].ToString().Should().Be("42");
    }

    [Fact]
    public async Task GetPageAsync_WhenIncludeCountFalse_DoesNotQueryCountOrWriteHeader()
    {
        PagingService service = CreateService(new PagingOptions());
        HttpResponse response = new DefaultHttpContext().Response;
        bool countInvoked = false;

        await service.GetPageAsync(
            new PageRequest(AfterId: 0, Limit: 10, IncludeCount: false),
            response,
            (afterId, limit, _) => Task.FromResult(SamplePage),
            _ =>
            {
                countInvoked = true;
                return Task.FromResult(42);
            },
            TestContext.Current.CancellationToken);

        countInvoked.Should().BeFalse();
        response.Headers.ContainsKey(PagingService.TotalCountHeaderName).Should().BeFalse();
    }

    #endregion

    #region Private Method Declarations

    private static PagingService CreateService(PagingOptions options)
    {
        return new PagingService(Options.Create(options));
    }

    #endregion
}
