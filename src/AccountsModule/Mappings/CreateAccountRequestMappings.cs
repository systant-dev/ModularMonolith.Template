using AccountsModule.Contracts.Requests;
using AccountsModule.Models;
using ModularMonolith.Template.MappingGen;

namespace AccountsModule.Mappings;

internal static partial class CreateAccountRequestMappings
{
    #region Public Method Declarations

    [Map]
    public static partial Account ToAccount(this CreateAccountRequest source);

    #endregion
}
