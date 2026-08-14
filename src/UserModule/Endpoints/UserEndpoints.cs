using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using UserModule.Contracts.Requests;
using UserModule.Contracts.Responses;
using UserModule.Services;

namespace UserModule.Endpoints;

internal static class UserEndpoints
{
    #region Endpoint Handlers

    // When a page size is supplied the endpoint returns a keyset (seek) page starting after
    // 'afterId'; otherwise it returns the full collection for backward compatibility. The page size
    // is clamped to a sane maximum to protect the database from unbounded reads.
    public static async Task<Ok<IReadOnlyCollection<UserResponse>>> GetAll(IUserService userService, CancellationToken cancellationToken, int? afterId = null, int? limit = null)
    {
        if (limit is null)
        {
            return TypedResults.Ok(await userService.GetAllAsync(cancellationToken));
        }

        int clampedLimit = Math.Clamp(limit.Value, 1, 100);
        return TypedResults.Ok(await userService.GetPageAsync(afterId ?? 0, clampedLimit, cancellationToken));
    }

    public static async Task<Results<Ok<UserResponse>, NotFound>> Get(int id, IUserService userService, CancellationToken cancellationToken)
    {
        return await userService.FindAsync(id, cancellationToken) is { } user ? TypedResults.Ok(user) : TypedResults.NotFound();
    }

    public static async Task<Created<UserResponse>> Create(CreateUserRequest request, IUserService userService, CancellationToken cancellationToken)
    {
        UserResponse created = await userService.CreateAsync(request, cancellationToken);
        return TypedResults.Created($"/users/{created.Id}", created);
    }

    public static async Task<Results<Ok<UserResponse>, NotFound>> Update(int id, UpdateUserRequest request, IUserService userService, CancellationToken cancellationToken)
    {
        return await userService.UpdateAsync(id, request, cancellationToken) is { } updated ? TypedResults.Ok(updated) : TypedResults.NotFound();
    }

    public static async Task<Results<NoContent, NotFound>> Delete(int id, IUserService userService, CancellationToken cancellationToken)
    {
        return await userService.DeleteAsync(id, cancellationToken) ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    #endregion
}
