using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserModule.Contracts.Events;
using UserModule.Contracts.Requests;
using UserModule.Contracts.Responses;
using UserModule.Mappings;
using UserModule.Models;
using UserModule.Storage;

namespace UserModule.Services;

internal sealed class UserService(UserDbContext context, ILogger<UserService> logger) : IUserService
{
    #region Public Method Declarations

    public async Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<User> users = await context.Users
            .AsNoTracking()
            .OrderBy(user => user.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return [.. users.Select(user => user.ToResponse())];
    }

    public async Task<UserResponse?> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        User? user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken)
            .ConfigureAwait(false);

        return user?.ToResponse();
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        User created = request.ToUser();
        context.Users.Add(created);

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        context.EnqueueOutboxMessage(new UserCreatedIntegrationEvent
        {
            Id = created.Id,
            FullName = created.FullName,
            Email = created.Email
        });

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        UserLog.UserCreated(logger, created);
        return created.ToResponse();
    }

    public async Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        User? existing = await context.Users
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return null;
        }

        existing.FullName = request.FullName;
        existing.Email = request.Email;
        existing.PhoneNumber = request.PhoneNumber;

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        UserLog.UserUpdated(logger, existing);
        return existing.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        User? existing = await context.Users
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return false;
        }

        context.Users.Remove(existing);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    #endregion
}
