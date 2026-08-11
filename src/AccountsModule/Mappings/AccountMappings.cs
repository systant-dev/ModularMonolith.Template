using AccountsModule.Contracts.Responses;
using AccountsModule.Models;
using ModularMonolith.Template.MappingGen;

namespace AccountsModule.Mappings;

internal static partial class AccountMappings
{
    #region Public Method Declarations

    [Map]
    public static partial AccountResponse ToResponse(this Account source);

    #endregion
}
