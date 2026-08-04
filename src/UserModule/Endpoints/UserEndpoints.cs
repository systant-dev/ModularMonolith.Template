using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using UserModule.Contracts.Requests;
using UserModule.Contracts.Responses;
using UserModule.Services;

namespace UserModule.Endpoints;

internal static class UserEndpoints
{
    #region Endpoint Handlers

    public static Ok<IReadOnlyCollection<UserResponse>> GetAll(IUserService userService) => TypedResults.Ok(userService.GetAll());

    public static Results<Ok<UserResponse>, NotFound> Get(int id, IUserService userService) => userService.Find(id) is { } user ? TypedResults.Ok(user) : TypedResults.NotFound();

    public static Created<UserResponse> Create(CreateUserRequest request, IUserService userService)
    {
        UserResponse created = userService.Create(request);
        return TypedResults.Created($"/users/{created.Id}", created);
    }

    public static Results<Ok<UserResponse>, NotFound> Update(int id, UpdateUserRequest request, IUserService userService) =>
        userService.Update(id, request) is { } updated ? TypedResults.Ok(updated) : TypedResults.NotFound();

    public static Results<NoContent, NotFound> Delete(int id, IUserService userService) =>
        userService.Delete(id) ? TypedResults.NoContent() : TypedResults.NotFound();

    #endregion
}