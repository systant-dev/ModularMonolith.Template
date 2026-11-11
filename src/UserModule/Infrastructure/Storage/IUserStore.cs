using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Results;
using UserModule.Infrastructure.Models;

namespace UserModule.Infrastructure.Storage;

internal interface IUserStore
{
    #region Method Declarations

    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetPageAsync(int afterId, int limit, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task<User?> FindAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, int excludingUserId, CancellationToken cancellationToken = default);

    Task<Result<User>> CreateAsync(User user, Func<User, IMessage> integrationEventFactory, CancellationToken cancellationToken = default);

    Task<Result<User>> UpdateAsync(User user, Func<User, IMessage> integrationEventFactory, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, Func<int, IMessage> integrationEventFactory, CancellationToken cancellationToken = default);

    #endregion
}
