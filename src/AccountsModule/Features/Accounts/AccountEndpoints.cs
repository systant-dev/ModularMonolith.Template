using AccountsModule.Contracts.Requests;
using AccountsModule.Contracts.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ModularMonolith.Template.SharedKernel.Paging;
using ModularMonolith.Template.SharedKernel.Results;

namespace AccountsModule.Features.Accounts;

internal static class AccountEndpoints
{
    #region Constant Declarations

    public const string GetRouteName = "GetAccount";

    #endregion

    #region Public Method Declarations

    public static async Task<Ok<IReadOnlyCollection<AccountResponse>>> GetAll(IAccountService accountService, IPagingService pagingService, HttpResponse response, CancellationToken cancellationToken, int? afterId = null, int? limit = null, bool includeCount = false)
    {
        PageRequest pageRequest = new(afterId, limit, includeCount);
        IReadOnlyCollection<AccountResponse> page = await pagingService.GetPageAsync(pageRequest, response, accountService.GetPageAsync, accountService.CountAsync, cancellationToken);
        return TypedResults.Ok(page);
    }

    public static async Task<Results<Ok<AccountResponse>, NotFound>> Get(int id, IAccountService accountService, CancellationToken cancellationToken)
    {
        Result<AccountResponse> result = await accountService.FindAsync(id, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : TypedResults.NotFound();
    }

    public static async Task<Results<CreatedAtRoute<AccountResponse>, ProblemHttpResult>> Create(CreateAccountRequest request, IAccountService accountService, CancellationToken cancellationToken)
    {
        Result<AccountResponse> result = await accountService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? TypedResults.CreatedAtRoute(result.Value, GetRouteName, new { id = result.Value.Id })
                                : result.Error.ToProblem();
    }

    public static async Task<Results<Ok<AccountResponse>, ProblemHttpResult>> Update(int id, UpdateAccountRequest request, IAccountService accountService, CancellationToken cancellationToken)
    {
        Result<AccountResponse> result = await accountService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();
    }

    public static async Task<Results<NoContent, ProblemHttpResult>> Delete(int id, IAccountService accountService, CancellationToken cancellationToken)
    {
        Result result = await accountService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.Error.ToProblem();
    }

    #endregion
}
