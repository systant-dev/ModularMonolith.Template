using AccountsModule.Contracts.Requests;
using AccountsModule.Contracts.Responses;

namespace AccountsModule.Services;

internal interface IAccountService
{
    #region Method Declarations

    Task<IReadOnlyCollection<AccountResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<AccountResponse?> FindAsync(int id, CancellationToken cancellationToken = default);

    Task<AccountResponse> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken = default);

    Task<AccountResponse?> UpdateAsync(int id, UpdateAccountRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion
}
