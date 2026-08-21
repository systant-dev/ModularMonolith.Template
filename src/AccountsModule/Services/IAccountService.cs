using AccountsModule.Contracts.Requests;
using AccountsModule.Contracts.Responses;
using ModularMonolith.Template.SharedKernel.Results;

namespace AccountsModule.Services;

internal interface IAccountService
{
    #region Method Declarations

    Task<IReadOnlyCollection<AccountResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<AccountResponse?> FindAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<AccountResponse>> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken = default);

    Task<Result<AccountResponse>> UpdateAsync(int id, UpdateAccountRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion
}
