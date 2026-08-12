using Microsoft.Extensions.Logging;
using UserModule.Contracts.Events;
using UserModule.Contracts.Requests;
using UserModule.Contracts.Responses;
using UserModule.Mappings;
using UserModule.Models;
using UserModule.Storage;

namespace UserModule.Services;

internal sealed class UserService(
    IUserStore store,
    ILogger<UserService> logger) : IUserService
{
    #region Public Method Declarations

    public async Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<User> users = await store.GetAllAsync(cancellationToken);
        return [.. users.Select(user => user.ToResponse())];
    }

    public async Task<IReadOnlyCollection<UserResponse>> GetPageAsync(int afterId, int limit, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<User> users = await store.GetPageAsync(afterId, limit, cancellationToken);
        return [.. users.Select(user => user.ToResponse())];
    }

    public async Task<UserResponse?> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        User? user = await store.FindAsync(id, cancellationToken);
        return user?.ToResponse();
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        User toCreate = request.ToUser();
        User created = await store.CreateAsync(
            toCreate,
            user => new UserCreatedIntegrationEvent
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email
            },
            cancellationToken);

        UserLog.UserCreated(logger, created);
        return created.ToResponse();
    }

    public async Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        User? existing = await store.FindAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        User updated = existing with
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };
        await store.UpdateAsync(updated, cancellationToken);

        UserLog.UserUpdated(logger, updated);
        return updated.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await store.DeleteAsync(id, cancellationToken);
    }

    #endregion
}
