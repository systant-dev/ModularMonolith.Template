using ModularMonolith.Template.MappingGen;
using AccountsModule.Contracts.Responses;
using AccountsModule.Models;

namespace AccountsModule.Mappings;

internal static partial class AccountMappings
{
    #region Public Method Declarations

    [Map]
    public static partial AccountResponse ToResponse(this Account source);

    #endregion
}
