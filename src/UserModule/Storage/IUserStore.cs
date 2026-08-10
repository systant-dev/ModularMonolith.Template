using ModularMonolith.Template.SharedKernel.Messaging;
using UserModule.Models;

namespace UserModule.Storage;

/// <summary>
/// Persistence abstraction for the UserModule. Implementations own their own transaction handling
/// so that <see cref="CreateAsync"/> can atomically insert the user and enqueue the resulting
/// integration event. The production implementation is backed by Dapper over SQL Server; tests
/// substitute a dependency-free in-memory implementation.
/// </summary>
internal interface IUserStore
{
    #region Method Declarations

    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<User?> FindAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts the user and enqueues an integration event within a single transaction. The event
    /// is produced by <paramref name="integrationEventFactory"/> from the persisted user so it can
    /// carry the assigned identifier. Returns the persisted user.
    /// </summary>
    Task<User> CreateAsync(User user, Func<User, IMessage> integrationEventFactory, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion
}
