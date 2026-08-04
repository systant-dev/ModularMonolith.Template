using UserModule.Contracts.Requests;
using UserModule.Contracts.Responses;

namespace UserModule.Services;

internal interface IUserService
{
    #region Method Declarations

    IReadOnlyCollection<UserResponse> GetAll();

    UserResponse? Find(int id);

    UserResponse Create(CreateUserRequest request);

    UserResponse? Update(int id, UpdateUserRequest request);

    bool Delete(int id);

    #endregion
}
