using AccountsModule.Contracts.Requests;
using AccountsModule.Contracts.Responses;
using AccountsModule.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

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

    public static async Task<Created<AccountResponse>> Create(CreateAccountRequest request, IAccountService accountService, CancellationToken cancellationToken)
    {
        AccountResponse created = await accountService.CreateAsync(request, cancellationToken);
        return TypedResults.Created($"/accounts/{created.Id}", created);
    }

    public static async Task<Results<Ok<AccountResponse>, NotFound>> Update(int id, UpdateAccountRequest request, IAccountService accountService, CancellationToken cancellationToken)
    {
        return await accountService.UpdateAsync(id, request, cancellationToken) is { } updated ? TypedResults.Ok(updated) : TypedResults.NotFound();
    }

    public static async Task<Results<NoContent, NotFound>> Delete(int id, IAccountService accountService, CancellationToken cancellationToken)
    {
        return await accountService.DeleteAsync(id, cancellationToken) ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    #endregion
}
