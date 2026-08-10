using System.Collections.Concurrent;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;
using UserModule.Models;
using UserModule.Storage;

namespace ModularMonolith.Template.UserModule.Tests.Storage;

/// <summary>
/// Dependency-free <see cref="IUserStore"/> and <see cref="IOutboxStore"/> used by tests. Keeps
/// users and outbox messages in memory so the suite runs without a SQL Server / LocalDB instance,
/// while still exercising the transactional create + outbox enqueue behaviour of the service.
/// </summary>
internal sealed class InMemoryUserStore : IUserStore, IOutboxStore
{
    #region Field Declarations

    private readonly ConcurrentDictionary<int, User> _users = new();
    private readonly ConcurrentDictionary<Guid, OutboxMessage> _outbox = new();
    private int _nextId;

    #endregion

    #region IUserStore Method Declarations

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<User> users = [.. _users.Values.OrderBy(user => user.Id)];
        return Task.FromResult(users);
    }

    public Task<User?> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        _users.TryGetValue(id, out User? user);
        return Task.FromResult(user);
    }

    public Task<User> CreateAsync(User user, Func<User, IMessage> integrationEventFactory, CancellationToken cancellationToken = default)
    {
        int id = Interlocked.Increment(ref _nextId);
        User created = user with { Id = id };
        _users[id] = created;

        OutboxMessage message = OutboxMessage.Create(integrationEventFactory(created));
        _outbox[message.Id] = message;

        return Task.FromResult(created);
    }

    public Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        if (!_users.ContainsKey(user.Id))
        {
            return Task.FromResult(false);
        }

        _users[user.Id] = user;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.TryRemove(id, out _));
    }

    #endregion

    #region IOutboxStore Method Declarations

    public Task EnqueueAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        OutboxMessage outboxMessage = OutboxMessage.Create(message);
        _outbox[outboxMessage.Id] = outboxMessage;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxAttempts, int batchSize, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<OutboxMessage> pending =
        [
            .. _outbox.Values
                .Where(message => message.ProcessedOnUtc is null && message.Attempts < maxAttempts)
                .OrderBy(message => message.OccurredOnUtc)
                .Take(batchSize)
        ];

        return Task.FromResult(pending);
    }

    public Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _outbox[message.Id] = message;
        return Task.CompletedTask;
    }

    #endregion
}
