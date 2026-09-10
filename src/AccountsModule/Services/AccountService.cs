using AccountsModule.Contracts.Events;
using AccountsModule.Contracts.Requests;
using AccountsModule.Contracts.Responses;
using AccountsModule.Mappings;
using AccountsModule.Models;
using AccountsModule.Storage;
using Microsoft.Extensions.Logging;
using ModularMonolith.Template.SharedKernel.Results;

namespace AccountsModule.Services;

internal sealed class AccountService(IAccountStore store, ILogger<AccountService> logger) : IAccountService
{
    #region Constant Declarations

    private const int MaxAccountNumberAttempts = 5;

    #endregion

    #region Public Method Declarations

    public async Task<IReadOnlyCollection<AccountResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Account> accounts = await store.GetAllAsync(cancellationToken);
        return [.. accounts.Select(account => account.ToResponse())];
    }

    public async Task<Result<AccountResponse>> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        Account? account = await store.FindAsync(id, cancellationToken);
        return account is null
            ? Error.NotFound("account.not-found", $"Account {id} was not found.")
            : account.ToResponse();
    }

    public async Task<Result<AccountResponse>> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken = default)
    {
        if (await store.ExistsForUserAsync(request.UserId, cancellationToken))
        {
            return Error.Conflict("account.already-exists", $"An account already exists for user {request.UserId}.");
        }

        Account toCreate = request.ToAccount();
        toCreate.Balance = request.InitialDeposit;

        for (int attempt = 1; attempt <= MaxAccountNumberAttempts; attempt++)
        {
            toCreate.AccountNumber = GenerateAccountNumber();

            Result<Account> createResult = await store.CreateAsync(
                toCreate,
                account => new AccountOpenedIntegrationEvent
                {
                    AccountId = account.Id,
                    UserId = account.UserId,
                    AccountNumber = account.AccountNumber
                },
                cancellationToken);

            if (createResult.IsFailure)
            {
                if (createResult.Error.Code != "account.number-conflict")
                {
                    return createResult.Error;
                }

                AccountLog.AccountNumberCollision(logger, attempt);
                continue;
            }

            Account created = createResult.Value;

            AccountLog.AccountOpened(logger, created);
            return created.ToResponse();
        }

        AccountLog.AccountNumberAllocationFailed(logger, MaxAccountNumberAttempts);
        return Error.Conflict("account.number-generation-failed", "Unable to allocate a unique account number. Please retry.");
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
        Result<Account> updateResult = await store.UpdateAsync(updated, cancellationToken);
        if (updateResult.IsFailure)
        {
            return updateResult.Error;
        }

        AccountLog.AccountUpdated(logger, updated);
        return updateResult.Value.ToResponse();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await store.DeleteAsync(id, cancellationToken))
        {
            return Error.NotFound("account.not-found", $"Account {id} was not found.");
        }

        return Result.Success();
    }

    #endregion

    #region Private Method Declarations

    private static string GenerateAccountNumber()
    {
        return $"SA{Random.Shared.NextInt64(0, 10_000_000_000L):D10}";
    }

    #endregion
}
