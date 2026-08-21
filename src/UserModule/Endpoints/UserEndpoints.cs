using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ModularMonolith.Template.SharedKernel.Results;
using UserModule.Contracts.Requests;
using UserModule.Contracts.Responses;
using UserModule.Services;

namespace UserModule.Endpoints;

internal static class UserEndpoints
{
    #region Public Method Declarations

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

    public static async Task<Results<Created<UserResponse>, ProblemHttpResult>> Create(CreateUserRequest request, IUserService userService, CancellationToken cancellationToken)
    {
        Result<UserResponse> result = await userService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? TypedResults.Created($"/users/{result.Value.Id}", result.Value) : result.Error.ToProblem();
    }

    public static async Task<Results<Ok<UserResponse>, ProblemHttpResult>> Update(int id, UpdateUserRequest request, IUserService userService, CancellationToken cancellationToken)
    {
        Result<UserResponse> result = await userService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();
    }

    public static async Task<Results<NoContent, NotFound>> Delete(int id, IUserService userService, CancellationToken cancellationToken)
    {
        return await userService.DeleteAsync(id, cancellationToken) ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    #endregion
}
