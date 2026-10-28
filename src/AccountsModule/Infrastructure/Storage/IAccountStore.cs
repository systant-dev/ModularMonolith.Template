using AccountsModule.Infrastructure.Models;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Results;

namespace AccountsModule.Infrastructure.Storage;

internal interface IAccountStore
{
    #region Method Declarations

    Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Account?> FindAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsForUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<Result<Account>> CreateAsync(Account account, Func<Account, IMessage> integrationEventFactory, CancellationToken cancellationToken = default);

    Task<Result<Account>> UpdateAsync(Account account, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion
}
