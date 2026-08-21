using Microsoft.Extensions.Logging;
using ModularMonolith.Template.SharedKernel.Results;
using UserModule.Contracts.Events;
using UserModule.Contracts.Requests;
using UserModule.Contracts.Responses;
using UserModule.Mappings;
using UserModule.Models;
using UserModule.Storage;

namespace UserModule.Services;

internal sealed class UserService(IUserStore store, ILogger<UserService> logger) : IUserService
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

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return store.CountAsync(cancellationToken);
    }

    public async Task<UserResponse?> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        User? user = await store.FindAsync(id, cancellationToken);
        return user?.ToResponse();
    }

    public async Task<Result<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (await store.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            return Error.Conflict("user.email-conflict", $"A user with email '{request.Email}' already exists.");
        }

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

    public async Task<Result<UserResponse>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        User? existing = await store.FindAsync(id, cancellationToken);
        if (existing is null)
        {
            return Error.NotFound("user.not-found", $"User {id} was not found.");
        }

        if (await store.ExistsByEmailAsync(request.Email, id, cancellationToken))
        {
            return Error.Conflict("user.email-conflict", $"A user with email '{request.Email}' already exists.");
        }

        User updated = existing with
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };

        bool wasUpdated = await store.UpdateAsync(
            updated,
            user => new UserUpdatedIntegrationEvent
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email
            },
            cancellationToken);

        if (!wasUpdated)
        {
            return Error.NotFound("user.not-found", $"User {id} was not found.");
        }

        UserLog.UserUpdated(logger, updated);
        return updated.ToResponse();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        bool wasDeleted = await store.DeleteAsync(
            id,
            deletedId => new UserDeletedIntegrationEvent { Id = deletedId },
            cancellationToken);

        if (!wasDeleted)
        {
            return Error.NotFound("user.not-found", $"User {id} was not found.");
        }

        UserLog.UserDeleted(logger, id);
        return Result.Success();
    }

    #endregion
}
