using AccountsModule.Contracts.Requests;
using AccountsModule.Contracts.Responses;
using AccountsModule.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ModularMonolith.Template.SharedKernel.Results;

namespace AccountsModule.Endpoints;

internal static class AccountEndpoints
{
    #region Endpoint Handlers

    public static async Task<Ok<IReadOnlyCollection<AccountResponse>>> GetAll(IAccountService accountService, CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await accountService.GetAllAsync(cancellationToken));
    }

    public static async Task<Results<Ok<AccountResponse>, NotFound>> Get(int id, IAccountService accountService, CancellationToken cancellationToken)
    {
        return await accountService.FindAsync(id, cancellationToken) is { } account ? TypedResults.Ok(account) : TypedResults.NotFound();
    }

    public static async Task<Results<Created<AccountResponse>, ProblemHttpResult>> Create(CreateAccountRequest request, IAccountService accountService, CancellationToken cancellationToken)
    {
        Result<AccountResponse> result = await accountService.CreateAsync(request, cancellationToken);
        return result.IsSuccess
            ? TypedResults.Created($"/accounts/{result.Value.Id}", result.Value)
            : result.Error.ToProblem();
    }

    public static async Task<Results<Ok<AccountResponse>, ProblemHttpResult>> Update(int id, UpdateAccountRequest request, IAccountService accountService, CancellationToken cancellationToken)
    {
        Result<AccountResponse> result = await accountService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();
    }

    public static async Task<Results<NoContent, NotFound>> Delete(int id, IAccountService accountService, CancellationToken cancellationToken)
    {
        return await accountService.DeleteAsync(id, cancellationToken) ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    #endregion
}
