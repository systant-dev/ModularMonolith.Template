using Microsoft.AspNetCore.Http;

namespace ModularMonolith.Template.SharedKernel.Paging;

public interface IPagingService
{
    #region Method Declarations

    Task<IReadOnlyCollection<TItem>> GetPageAsync<TItem>(PageRequest request,
                                                         HttpResponse response,
                                                         Func<int, int, CancellationToken, Task<IReadOnlyCollection<TItem>>> fetchPageAsync,
                                                         Func<CancellationToken, Task<int>> countAsync,
                                                         CancellationToken cancellationToken = default);

    #endregion
}
