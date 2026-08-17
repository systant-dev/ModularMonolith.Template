using AccountsModule.Contracts.Events;
using AccountsModule.Contracts.Requests;
using AccountsModule.Contracts.Responses;
using AccountsModule.Mappings;
using AccountsModule.Models;
using AccountsModule.Storage;
using Microsoft.Extensions.Logging;
using ModularMonolith.Template.SharedKernel.Results;

namespace AccountsModule.Services;

internal sealed class AccountService(
    IAccountStore store,
    ILogger<AccountService> logger) : IAccountService
{
    #region Public Method Declarations

    public async Task<IReadOnlyCollection<AccountResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Account> accounts = await store.GetAllAsync(cancellationToken);
        return [.. accounts.Select(account => account.ToResponse())];
    }

    public async Task<AccountResponse?> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        Account? account = await store.FindAsync(id, cancellationToken);
        return account?.ToResponse();
    }

    public async Task<Result<AccountResponse>> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken = default)
    {
        if (await store.ExistsForUserAsync(request.UserId, cancellationToken))
        {
            return Error.Conflict("account.already-exists", $"An account already exists for user {request.UserId}.");
        }

        Account toCreate = request.ToAccount();
        toCreate.AccountNumber = GenerateAccountNumber();
        toCreate.Balance = request.InitialDeposit;
        Account created = await store.CreateAsync(
            toCreate,
            account => new AccountOpenedIntegrationEvent
            {
                AccountId = account.Id,
                UserId = account.UserId,
                AccountNumber = account.AccountNumber
            },
            cancellationToken);

        AccountLog.AccountOpened(logger, created);
        return created.ToResponse();
    }

    public async Task<Result<AccountResponse>> UpdateAsync(int id, UpdateAccountRequest request, CancellationToken cancellationToken = default)
    {
        Account? existing = await store.FindAsync(id, cancellationToken);
        if (existing is null)
        {
            return Error.NotFound("account.not-found", $"Account {id} was not found.");
        }

        Account updated = existing with
        {
            AccountHolderName = request.AccountHolderName
        };
        await store.UpdateAsync(updated, cancellationToken);

        AccountLog.AccountUpdated(logger, updated);
        return updated.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await store.DeleteAsync(id, cancellationToken);
    }

    #endregion

    #region Private Method Declarations

    private static string GenerateAccountNumber()
    {
        return $"SA{Random.Shared.NextInt64(0, 10_000_000_000L):D10}";
    }

    #endregion
}
