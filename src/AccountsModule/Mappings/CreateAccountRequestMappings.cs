using ModularMonolith.Template.MappingGen;
using AccountsModule.Contracts.Requests;
using AccountsModule.Models;

namespace AccountsModule.Mappings;

internal static partial class CreateAccountRequestMappings
{
    #region Public Method Declarations

    [Map]
    public static partial Account ToAccount(this CreateAccountRequest source);

    #endregion
}
