using Microsoft.Extensions.Logging;
using ModularMonolith.Template.SharedKernel.Results;
using UserModule.Contracts.Events;
using UserModule.Contracts.Requests;
using UserModule.Contracts.Responses;
using UserModule.Features.Users.Mapping;
using UserModule.Infrastructure.Models;
using UserModule.Infrastructure.Storage;

namespace UserModule.Features.Users;

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

    public async Task<Result<UserResponse>> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        User? user = await store.FindAsync(id, cancellationToken);
        return user is null
            ? Error.NotFound("user.not-found", $"User {id} was not found.")
            : user.ToResponse();
    }

    public async Task<Result<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (await EmailAlreadyExistsAsync(request.Email, cancellationToken))
        {
            return Error.Conflict("user.email-conflict", $"A user with email '{request.Email}' already exists.");
        }

        Result<User> createResult = await store.CreateAsync(
            CreateUser(request),
            CreateUserCreatedEvent,
            cancellationToken);

        if (createResult.IsFailure)
        {
            return createResult.Error;
        }

        User created = createResult.Value;
        UserLog.UserCreated(logger, created);
        return created.ToResponse();
    }

    public async Task<Result<UserResponse>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        User? existing = await FindUserAsync(id, cancellationToken);
        if (existing is null)
        {
            return Error.NotFound("user.not-found", $"User {id} was not found.");
        }

        if (await EmailAlreadyExistsAsync(request.Email, id, cancellationToken))
        {
            return Error.Conflict("user.email-conflict", $"A user with email '{request.Email}' already exists.");
        }

        User updated = UpdateUser(existing, request);

        Result<User> updateResult = await store.UpdateAsync(
            updated,
            CreateUserUpdatedEvent,
            cancellationToken);

        if (updateResult.IsFailure)
        {
            return updateResult.Error;
        }

        UserLog.UserUpdated(logger, updated);
        return updated.ToResponse();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        bool wasDeleted = await store.DeleteAsync(id, CreateUserDeletedEvent, cancellationToken);

        if (!wasDeleted)
        {
            return Error.NotFound("user.not-found", $"User {id} was not found.");
        }

        UserLog.UserDeleted(logger, id);
        return Result.Success();
    }

    #endregion

    #region Private Method Declarations

    private Task<User?> FindUserAsync(int id, CancellationToken cancellationToken)
    {
        return store.FindAsync(id, cancellationToken);
    }

    private Task<bool> EmailAlreadyExistsAsync(string email, CancellationToken cancellationToken)
    {
        return store.ExistsByEmailAsync(email, cancellationToken);
    }

    private Task<bool> EmailAlreadyExistsAsync(string email, int excludingUserId, CancellationToken cancellationToken)
    {
        return store.ExistsByEmailAsync(email, excludingUserId, cancellationToken);
    }

    private static User CreateUser(CreateUserRequest request)
    {
        return request.ToUser();
    }

    private static User UpdateUser(User existing, UpdateUserRequest request)
    {
        return existing with
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };
    }

    private static UserCreatedIntegrationEvent CreateUserCreatedEvent(User user)
    {
        return new UserCreatedIntegrationEvent
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email
        };
    }

    private static UserUpdatedIntegrationEvent CreateUserUpdatedEvent(User user)
    {
        return new UserUpdatedIntegrationEvent
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email
        };
    }

    private static UserDeletedIntegrationEvent CreateUserDeletedEvent(int userId)
    {
        return new UserDeletedIntegrationEvent { Id = userId };
    }

    #endregion
}
