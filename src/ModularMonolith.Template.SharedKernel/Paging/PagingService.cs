using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace ModularMonolith.Template.SharedKernel.Paging;

internal sealed class PagingService(IOptions<PagingOptions> options) : IPagingService
{
    #region Constant Declarations

    public const string TotalCountHeaderName = "X-Total-Count";

    #endregion

    #region Field Declarations

    private readonly PagingOptions _options = options.Value;

    #endregion

    #region Public Method Declarations

    public async Task<IReadOnlyCollection<TItem>> GetPageAsync<TItem>(PageRequest request,
                                                                      HttpResponse response,
                                                                      Func<int, int, CancellationToken, Task<IReadOnlyCollection<TItem>>> fetchPageAsync,
                                                                      Func<CancellationToken, Task<int>> countAsync,
                                                                      CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(fetchPageAsync);

        int afterId = Math.Max(request.AfterId ?? 0, 0);
        int limit = Math.Clamp(request.Limit ?? _options.DefaultLimit, 1, _options.MaxLimit);

        IReadOnlyCollection<TItem> page = await fetchPageAsync(afterId, limit, cancellationToken);

        if (request.IncludeCount)
        {
            ArgumentNullException.ThrowIfNull(countAsync);
            int total = await countAsync(cancellationToken);
            response.Headers[TotalCountHeaderName] = total.ToString(CultureInfo.InvariantCulture);
        }

        return page;
    }

    #endregion
}
