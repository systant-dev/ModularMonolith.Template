using ModularMonolith.Template.SharedKernel.Messaging;
using UserModule.Models;

namespace UserModule.Storage;

internal interface IUserStore
{
    #region Method Declarations

    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetPageAsync(int afterId, int limit, CancellationToken cancellationToken = default);

    Task<User?> FindAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User> CreateAsync(User user, Func<User, IMessage> integrationEventFactory, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion
}
