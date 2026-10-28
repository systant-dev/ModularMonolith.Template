using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ModularMonolith.Template.SharedKernel.Paging;
using ModularMonolith.Template.SharedKernel.Results;
using UserModule.Contracts.Requests;
using UserModule.Contracts.Responses;

namespace UserModule.Features.Users;

internal static class UserEndpoints
{
    #region Public Method Declarations

    public static async Task<Ok<IReadOnlyCollection<UserResponse>>> GetAll(IUserService userService, IPagingService pagingService, HttpResponse response, CancellationToken cancellationToken, int? afterId = null, int? limit = null, bool includeCount = false)
    {
        if (limit is null)
        {
            return TypedResults.Ok(await userService.GetAllAsync(cancellationToken));
        }

        IReadOnlyCollection<UserResponse> page = await pagingService.GetPageAsync(
            new PageRequest(afterId, limit, includeCount),
            response,
            userService.GetPageAsync,
            userService.CountAsync,
            cancellationToken);

        return TypedResults.Ok(page);
    }

    public static async Task<Results<Ok<UserResponse>, NotFound>> Get(int id, IUserService userService, CancellationToken cancellationToken)
    {
        Result<UserResponse> result = await userService.FindAsync(id, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : TypedResults.NotFound();
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

    public static async Task<Results<NoContent, ProblemHttpResult>> Delete(int id, IUserService userService, CancellationToken cancellationToken)
    {
        Result result = await userService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.Error.ToProblem();
    }

    #endregion
}
