using AccountsModule.Contracts.Requests;
using AccountsModule.Infrastructure.Models;
using ModularMonolith.Template.MappingGen;

namespace AccountsModule.Features.Accounts.Mapping;

internal static partial class CreateAccountRequestMappings
{
    #region Public Method Declarations

    [Map]
    public static partial Account ToAccount(this CreateAccountRequest source);

    #endregion
}
