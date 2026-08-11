using AccountsModule.Models;
using ModularMonolith.Template.SharedKernel.Messaging;

namespace AccountsModule.Storage;

internal interface IAccountStore
{
    #region Method Declarations

    Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Account?> FindAsync(int id, CancellationToken cancellationToken = default);

    Task<Account> CreateAsync(Account account, Func<Account, IMessage> integrationEventFactory, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Account account, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion
}
