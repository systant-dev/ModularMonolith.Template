using Microsoft.Extensions.Logging;
using UserModule.Contracts.Requests;
using UserModule.Contracts.Responses;
using UserModule.Mappings;
using UserModule.Models;
using UserModule.Storage;

namespace UserModule.Services;

internal sealed class UserService(UserStore store, ILogger<UserService> logger) : IUserService
{
    #region Public Method Declarations

    public IReadOnlyCollection<UserResponse> GetAll() =>
        [.. store.GetAll().Select(user => user.ToResponse())];

    public UserResponse? Find(int id) => store.Get(id)?.ToResponse();

    public UserResponse Create(CreateUserRequest request)
    {
        User created = store.Add(request.ToUser());
        UserLog.UserCreated(logger, created);
        return created.ToResponse();
    }

    public UserResponse? Update(int id, UpdateUserRequest request)
    {
        User? updated = store.Update(id, request.ToUser());
        if (updated is null)
        {
            return null;
        }

        UserLog.UserUpdated(logger, updated);
        return updated.ToResponse();
    }

    public bool Delete(int id) => store.Remove(id);

    #endregion
}
