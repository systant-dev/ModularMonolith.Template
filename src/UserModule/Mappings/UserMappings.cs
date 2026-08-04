using ModularMonolith.Template.MappingGen;
using UserModule.Contracts.Responses;
using UserModule.Models;

namespace UserModule.Mappings;

internal static partial class UserMappings
{
    #region Public Method Declarations

    [Map]
    public static partial UserResponse ToResponse(this User source);

    #endregion
}
