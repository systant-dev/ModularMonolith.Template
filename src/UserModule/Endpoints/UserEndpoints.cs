using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using UserModule.Contracts.Requests;
using UserModule.Contracts.Responses;
using UserModule.Services;

namespace UserModule.Endpoints;

internal static class UserEndpoints
{
    #region Endpoint Handlers

    public static async Task<Ok<IReadOnlyCollection<UserResponse>>> GetAll(IUserService userService, CancellationToken cancellationToken) =>
        TypedResults.Ok(await userService.GetAllAsync(cancellationToken));

    public static async Task<Results<Ok<UserResponse>, NotFound>> Get(int id, IUserService userService, CancellationToken cancellationToken) =>
        await userService.FindAsync(id, cancellationToken) is { } user ? TypedResults.Ok(user) : TypedResults.NotFound();

    public static async Task<Created<UserResponse>> Create(CreateUserRequest request, IUserService userService, CancellationToken cancellationToken)
    {
        UserResponse created = await userService.CreateAsync(request, cancellationToken);
        return TypedResults.Created($"/users/{created.Id}", created);
    }

    public static async Task<Results<Ok<UserResponse>, NotFound>> Update(int id, UpdateUserRequest request, IUserService userService, CancellationToken cancellationToken) =>
        await userService.UpdateAsync(id, request, cancellationToken) is { } updated ? TypedResults.Ok(updated) : TypedResults.NotFound();

    public static async Task<Results<NoContent, NotFound>> Delete(int id, IUserService userService, CancellationToken cancellationToken) =>
        await userService.DeleteAsync(id, cancellationToken) ? TypedResults.NoContent() : TypedResults.NotFound();

    #endregion
}