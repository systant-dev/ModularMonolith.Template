using ModularMonolith.Template.MappingGen;
using UserModule.Contracts.Responses;
using UserModule.Infrastructure.Models;

namespace UserModule.Features.Users.Mapping;

internal static partial class UserMappings
{
    #region Public Method Declarations

    [Map]
    public static partial UserResponse ToResponse(this User source);

    #endregion
}
