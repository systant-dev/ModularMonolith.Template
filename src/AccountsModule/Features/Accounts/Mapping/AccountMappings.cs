using AccountsModule.Contracts.Responses;
using AccountsModule.Infrastructure.Models;
using ModularMonolith.Template.MappingGen;

namespace AccountsModule.Features.Accounts.Mapping;

internal static partial class AccountMappings
{
    #region Public Method Declarations

    [Map]
    public static partial AccountResponse ToResponse(this Account source);

    #endregion
}
