using UserModule.Contracts.Requests;
using UserModule.Contracts.Responses;

namespace UserModule.Services;

internal interface IUserService
{
    #region Method Declarations

    Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserResponse>> GetPageAsync(int afterId, int limit, CancellationToken cancellationToken = default);

    Task<UserResponse?> FindAsync(int id, CancellationToken cancellationToken = default);

    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion
}
