using ModularMonolith.Template.MappingGen;
using UserModule.Contracts.Requests;
using UserModule.Infrastructure.Models;

namespace UserModule.Features.Users.Mapping;

internal static partial class UpdateUserRequestMappings
{
    #region Public Method Declarations

    [Map]
    public static partial User ToUser(this UpdateUserRequest source);

    #endregion
}
