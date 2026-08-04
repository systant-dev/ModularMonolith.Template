using ModularMonolith.Template.MappingGen;
using UserModule.Contracts.Requests;
using UserModule.Models;

namespace UserModule.Mappings;

internal static partial class UpdateUserRequestMappings
{
    #region Public Method Declarations

    [Map]
    public static partial User ToUser(this UpdateUserRequest source);

    #endregion
}
