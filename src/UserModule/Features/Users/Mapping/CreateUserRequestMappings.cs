using ModularMonolith.Template.MappingGen;
using UserModule.Contracts.Requests;
using UserModule.Infrastructure.Models;

namespace UserModule.Features.Users.Mapping;

internal static partial class CreateUserRequestMappings
{
    #region Public Method Declarations

    [Map]
    public static partial User ToUser(this CreateUserRequest source);

    #endregion
}
