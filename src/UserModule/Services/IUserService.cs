using UserModule.Contracts.Requests;
using UserModule.Contracts.Responses;
using ModularMonolith.Template.SharedKernel.Results;

namespace UserModule.Services;

internal interface IUserService
{
    #region Method Declarations

    Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserResponse>> GetPageAsync(int afterId, int limit, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task<Result<UserResponse>> FindAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<Result<UserResponse>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion
}
