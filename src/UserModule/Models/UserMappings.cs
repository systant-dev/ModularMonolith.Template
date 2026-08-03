using ModularMonolith.Template.MappingGen;

namespace UserModule.Models;

internal static partial class UserMappings
{
    #region Public Method Declarations

    [Map]
    public static partial TTarget To<TTarget>(this User source);

    #endregion
}
